using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using EnquiryEntity = SchoolERP.Domain.Student.Enquiry.Entities.Enquiry;
using EnquiryFollowupEntity = SchoolERP.Domain.Student.Enquiry.Entities.EnquiryFollowup;

namespace SchoolERP.Application.Features.Student.Queries.EnquiryFollowup.GetEnquiryFollowups;

public class GetEnquiryFollowupsQueryHandler : IRequestHandler<GetEnquiryFollowupsQuery, Result<List<EnquiryFollowupResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetEnquiryFollowupsQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<List<EnquiryFollowupResponse>>> Handle(GetEnquiryFollowupsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        // Verify parent enquiry belongs to current branch
        var enquiryExists = await _dbContext.Set<EnquiryEntity>()
            .AnyAsync(e => e.Id == query.EnquiryId && e.BranchId == branchId.Value && !e.IsDeleted, cancellationToken);

        if (!enquiryExists)
            return Error.NotFound("Enquiry", query.EnquiryId.ToString());

        var items = await _dbContext.Set<EnquiryFollowupEntity>()
            .AsNoTracking()
            .Where(f => f.EnquiryId == query.EnquiryId && !f.IsDeleted)
            .OrderByDescending(f => f.CreatedAt)
            .ProjectToType<EnquiryFollowupResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}
