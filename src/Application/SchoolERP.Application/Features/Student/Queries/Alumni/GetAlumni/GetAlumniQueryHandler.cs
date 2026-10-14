using MediatR;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using AlumniEntity = SchoolERP.Domain.Student.Alumni.Entities.Alumni;

namespace SchoolERP.Application.Features.Student.Queries.Alumni.GetAlumni;

public class GetAlumniQueryHandler : IRequestHandler<GetAlumniQuery, Result<PagedResponse<AlumniResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetAlumniQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<PagedResponse<AlumniResponse>>> Handle(GetAlumniQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var request = query.Request;

        var q = _dbContext.Set<AlumniEntity>()
            .AsNoTracking()
            .Where(a => a.BranchId == branchId && !a.IsDeleted);

        if (request.IsArchived.HasValue)
            q = q.Where(a => a.IsArchived == request.IsArchived.Value);
        else
            q = q.Where(a => !a.IsArchived);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var s = request.SearchTerm.ToLower();
            q = q.Where(a =>
                a.AlumniId.ToLower().Contains(s) ||
                a.FirstName.ToLower().Contains(s) ||
                (a.LastName != null && a.LastName.ToLower().Contains(s)) ||
                (a.ContactMobile != null && a.ContactMobile.Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
            q = q.Where(a => a.CurrentStatus == request.Status);

        if (request.LastClassId.HasValue)
            q = q.Where(a => a.LastClassId == request.LastClassId.Value);

        if (request.PassingAcademicYearId.HasValue)
            q = q.Where(a => a.PassingAcademicYearId == request.PassingAcademicYearId.Value);

        if (!string.IsNullOrWhiteSpace(request.Industry))
            q = q.Where(a => a.Industry == request.Industry);

        q = request.SortBy?.ToLower() switch
        {
            "firstname" => request.SortOrder?.ToLower() == "desc" ? q.OrderByDescending(a => a.FirstName) : q.OrderBy(a => a.FirstName),
            "graduationdate" => request.SortOrder?.ToLower() == "desc" ? q.OrderByDescending(a => a.GraduationDate) : q.OrderBy(a => a.GraduationDate),
            "alumniid" => request.SortOrder?.ToLower() == "desc" ? q.OrderByDescending(a => a.AlumniId) : q.OrderBy(a => a.AlumniId),
            _ => q.OrderByDescending(a => a.CreatedAt)
        };

        var totalCount = await q.CountAsync(cancellationToken);

        var rawItems = await q
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var classIds = rawItems.Select(x => x.LastClassId).Distinct().ToList();
        var ayIds = rawItems.Select(x => x.PassingAcademicYearId).Distinct().ToList();

        var classNames = await _dbContext.Set<ClassEntity>().AsNoTracking()
            .Where(c => classIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);
        var ayNames = await _dbContext.Set<AcademicYearEntity>().AsNoTracking()
            .Where(a => ayIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id, a => a.Name, cancellationToken);

        var data = rawItems.Select(a =>
        {
            var dto = a.Adapt<AlumniResponse>();
            return dto with
            {
                LastClassName = classNames.TryGetValue(a.LastClassId, out var cn) ? cn : null,
                PassingAcademicYearName = ayNames.TryGetValue(a.PassingAcademicYearId, out var ay) ? ay : null
            };
        }).ToList();

        return Result.Success(new PagedResponse<AlumniResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}
