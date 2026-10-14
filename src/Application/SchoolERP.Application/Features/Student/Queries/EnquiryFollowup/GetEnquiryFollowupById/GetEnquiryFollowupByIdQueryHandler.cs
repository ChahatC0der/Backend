using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using EnquiryEntity = SchoolERP.Domain.Student.Enquiry.Entities.Enquiry;
using EnquiryFollowupEntity = SchoolERP.Domain.Student.Enquiry.Entities.EnquiryFollowup;

namespace SchoolERP.Application.Features.Student.Queries.EnquiryFollowup.GetEnquiryFollowupById;

public class GetEnquiryFollowupByIdQueryHandler : IRequestHandler<GetEnquiryFollowupByIdQuery, Result<EnquiryFollowupResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetEnquiryFollowupByIdQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<EnquiryFollowupResponse>> Handle(GetEnquiryFollowupByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var followup = await (
            from f in _dbContext.Set<EnquiryFollowupEntity>()
            join e in _dbContext.Set<EnquiryEntity>() on f.EnquiryId equals e.Id
            where f.Id == query.Id && !f.IsDeleted && !e.IsDeleted && e.BranchId == branchId.Value
            select f
        ).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

        if (followup == null)
            return Error.NotFound("EnquiryFollowup", query.Id.ToString());

        return Result.Success(followup.Adapt<EnquiryFollowupResponse>());
    }
}
