using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using EnquiryEntity = SchoolERP.Domain.Student.Enquiry.Entities.Enquiry;

namespace SchoolERP.Application.Features.Student.Queries.Enquiry.GetEnquiriesLight;

public class GetEnquiriesLightQueryHandler : IRequestHandler<GetEnquiriesLightQuery, Result<List<EnquiryLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetEnquiriesLightQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<List<EnquiryLightResponse>>> Handle(GetEnquiriesLightQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var items = await _dbContext.Set<EnquiryEntity>()
            .AsNoTracking()
            .Where(e => e.BranchId == branchId.Value && !e.IsDeleted)
            .OrderByDescending(e => e.EnquiryDate)
            .ProjectToType<EnquiryLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}
