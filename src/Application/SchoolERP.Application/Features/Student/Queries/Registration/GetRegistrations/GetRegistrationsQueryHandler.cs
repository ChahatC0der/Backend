using MediatR;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;

namespace SchoolERP.Application.Features.Student.Queries.Registration.GetRegistrations;

public class GetRegistrationsQueryHandler : IRequestHandler<GetRegistrationsQuery, Result<PagedResponse<RegistrationResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetRegistrationsQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<PagedResponse<RegistrationResponse>>> Handle(GetRegistrationsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var request = query.Request;

        var q = _dbContext.Set<RegistrationEntity>()
            .AsNoTracking()
            .Where(r => r.BranchId == branchId.Value && !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var s = request.SearchTerm.ToLower();
            q = q.Where(r =>
                r.RegistrationNumber.ToLower().Contains(s) ||
                r.StudentFirstName.ToLower().Contains(s) ||
                (r.StudentLastName != null && r.StudentLastName.ToLower().Contains(s)) ||
                r.ParentPrimaryMobile.Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
            q = q.Where(r => r.Status == request.Status);

        if (request.ClassId.HasValue)
            q = q.Where(r => r.ClassId == request.ClassId.Value);

        if (request.AcademicYearId.HasValue)
            q = q.Where(r => r.AcademicYearId == request.AcademicYearId.Value);

        if (request.FromDate.HasValue)
            q = q.Where(r => r.CreatedAt >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            q = q.Where(r => r.CreatedAt <= request.ToDate.Value);

        q = request.SortBy?.ToLower() switch
        {
            "studentname" => request.SortOrder?.ToLower() == "desc" ? q.OrderByDescending(r => r.StudentFirstName) : q.OrderBy(r => r.StudentFirstName),
            "status" => request.SortOrder?.ToLower() == "desc" ? q.OrderByDescending(r => r.Status) : q.OrderBy(r => r.Status),
            "regnumber" => request.SortOrder?.ToLower() == "desc" ? q.OrderByDescending(r => r.RegistrationNumber) : q.OrderBy(r => r.RegistrationNumber),
            _ => q.OrderByDescending(r => r.CreatedAt)
        };

        var totalCount = await q.CountAsync(cancellationToken);

        var rawItems = await q
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var classIds = rawItems.Select(x => x.ClassId).Distinct().ToList();
        var sectionIds = rawItems.Where(x => x.SectionId.HasValue).Select(x => x.SectionId!.Value).Distinct().ToList();
        var userIds = rawItems.Where(x => x.InterviewerId.HasValue).Select(x => x.InterviewerId!.Value)
            .Concat(rawItems.Where(x => x.ApprovedBy.HasValue).Select(x => x.ApprovedBy!.Value))
            .Distinct().ToList();

        var classNames = await _dbContext.Set<ClassEntity>().AsNoTracking()
            .Where(c => classIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);
        var sectionNames = await _dbContext.Set<SectionEntity>().AsNoTracking()
            .Where(s => sectionIds.Contains(s.Id)).ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);
        var userNames = await _dbContext.Set<UserEntity>().AsNoTracking()
            .Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.Name, cancellationToken);

        var data = rawItems.Select(r =>
        {
            var dto = r.Adapt<RegistrationResponse>();
            return dto with
            {
                ClassName = classNames.TryGetValue(r.ClassId, out var cn) ? cn : null,
                SectionName = r.SectionId.HasValue && sectionNames.TryGetValue(r.SectionId.Value, out var sn) ? sn : null,
                InterviewerName = r.InterviewerId.HasValue && userNames.TryGetValue(r.InterviewerId.Value, out var inName) ? inName : null,
                ApprovedByName = r.ApprovedBy.HasValue && userNames.TryGetValue(r.ApprovedBy.Value, out var an) ? an : null
            };
        }).ToList();

        return Result.Success(new PagedResponse<RegistrationResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}
