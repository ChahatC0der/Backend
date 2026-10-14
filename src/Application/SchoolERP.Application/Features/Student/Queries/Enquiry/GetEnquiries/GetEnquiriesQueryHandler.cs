using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using EnquiryEntity = SchoolERP.Domain.Student.Enquiry.Entities.Enquiry;
using UserEntity = SchoolERP.Domain.Rbac.Entities.User;

namespace SchoolERP.Application.Features.Student.Queries.Enquiry.GetEnquiries;

public class GetEnquiriesQueryHandler : IRequestHandler<GetEnquiriesQuery, Result<PagedResponse<EnquiryResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetEnquiriesQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<PagedResponse<EnquiryResponse>>> Handle(GetEnquiriesQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var request = query.Request;

        var q = _dbContext.Set<EnquiryEntity>()
            .AsNoTracking()
            .Where(e => e.BranchId == branchId.Value && !e.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var s = request.SearchTerm.ToLower();
            q = q.Where(e =>
                e.ParentName.ToLower().Contains(s) ||
                e.Mobile.Contains(s) ||
                (e.StudentFirstName != null && e.StudentFirstName.ToLower().Contains(s)) ||
                (e.StudentLastName != null && e.StudentLastName.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
            q = q.Where(e => e.Status == request.Status);

        if (!string.IsNullOrWhiteSpace(request.Source))
            q = q.Where(e => e.Source == request.Source);

        if (request.ClassId.HasValue)
            q = q.Where(e => e.ClassId == request.ClassId.Value);

        if (request.AssignedTo.HasValue)
            q = q.Where(e => e.AssignedTo == request.AssignedTo.Value);

        if (request.FromDate.HasValue)
            q = q.Where(e => e.EnquiryDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            q = q.Where(e => e.EnquiryDate <= request.ToDate.Value);

        q = request.SortBy?.ToLower() switch
        {
            "parentname" => request.SortOrder?.ToLower() == "desc" ? q.OrderByDescending(e => e.ParentName) : q.OrderBy(e => e.ParentName),
            "status" => request.SortOrder?.ToLower() == "desc" ? q.OrderByDescending(e => e.Status) : q.OrderBy(e => e.Status),
            "enquirydate" => request.SortOrder?.ToLower() == "desc" ? q.OrderByDescending(e => e.EnquiryDate) : q.OrderBy(e => e.EnquiryDate),
            _ => q.OrderByDescending(e => e.EnquiryDate)
        };

        var totalCount = await q.CountAsync(cancellationToken);

        var rawItems = await q
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var classIds = rawItems.Where(x => x.ClassId.HasValue).Select(x => x.ClassId!.Value).Distinct().ToList();
        var userIds = rawItems.Where(x => x.AssignedTo.HasValue).Select(x => x.AssignedTo!.Value).Distinct().ToList();

        var classNames = await _dbContext.Set<ClassEntity>()
            .AsNoTracking()
            .Where(c => classIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var userNames = await _dbContext.Set<UserEntity>()
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Name, cancellationToken);

        var data = rawItems.Select(e => new EnquiryResponse(
            e.Id, e.BranchId, e.EnquiryDate, e.Source,
            e.StudentFirstName, e.StudentLastName,
            e.ParentName, e.Mobile, e.Email,
            e.ClassId,
            e.ClassId.HasValue && classNames.TryGetValue(e.ClassId.Value, out var cn) ? cn : null,
            e.Message,
            e.AssignedTo,
            e.AssignedTo.HasValue && userNames.TryGetValue(e.AssignedTo.Value, out var un) ? un : null,
            e.Status, e.ConvertedToRegistrationId,
            e.CreatedAt, e.UpdatedAt)).ToList();

        return Result.Success(new PagedResponse<EnquiryResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}
