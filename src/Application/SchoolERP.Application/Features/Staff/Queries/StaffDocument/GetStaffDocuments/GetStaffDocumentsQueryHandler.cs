using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;
using StaffDocumentEntity = SchoolERP.Domain.Staff.Entities.StaffDocument;

namespace SchoolERP.Application.Features.Staff.Queries.StaffDocument.GetStaffDocuments;

public class GetStaffDocumentsQueryHandler : IRequestHandler<GetStaffDocumentsQuery, Result<PagedResponse<StaffDocumentResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetStaffDocumentsQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<PagedResponse<StaffDocumentResponse>>> Handle(GetStaffDocumentsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;
        var request = query.Request;

        // Join with Staff to filter by branch
        var queryable = from doc in _dbContext.Set<StaffDocumentEntity>()
                        join staff in _dbContext.Set<StaffEntity>() on doc.StaffId equals staff.Id
                        where !doc.IsDeleted && !staff.IsDeleted && staff.BranchId == branchId
                        select doc;

        if (query.StaffId.HasValue)
            queryable = queryable.Where(d => d.StaffId == query.StaffId.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            queryable = queryable.Where(d => d.DocumentType.ToLower().Contains(search) ||
                                             d.VerificationStatus.ToLower().Contains(search));
        }

        queryable = request.SortBy?.ToLower() switch
        {
            "documenttype" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(d => d.DocumentType) : queryable.OrderBy(d => d.DocumentType),
            "expirydate" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(d => d.ExpiryDate) : queryable.OrderBy(d => d.ExpiryDate),
            _ => queryable.OrderBy(d => d.DocumentType)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var data = items.Select(x => x.Adapt<StaffDocumentResponse>()).ToList();

        return Result.Success(new PagedResponse<StaffDocumentResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}
