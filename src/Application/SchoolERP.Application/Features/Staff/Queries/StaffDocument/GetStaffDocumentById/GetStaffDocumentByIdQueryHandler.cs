using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;
using StaffDocumentEntity = SchoolERP.Domain.Staff.Entities.StaffDocument;

namespace SchoolERP.Application.Features.Staff.Queries.StaffDocument.GetStaffDocumentById;

public class GetStaffDocumentByIdQueryHandler : IRequestHandler<GetStaffDocumentByIdQuery, Result<StaffDocumentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetStaffDocumentByIdQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<StaffDocumentResponse>> Handle(GetStaffDocumentByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var document = await (
            from doc in _dbContext.Set<StaffDocumentEntity>()
            join staff in _dbContext.Set<StaffEntity>() on doc.StaffId equals staff.Id
            where doc.Id == query.Id && !doc.IsDeleted && !staff.IsDeleted && staff.BranchId == branchId
            select doc
        ).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

        if (document == null)
            return Error.NotFound("StaffDocument", query.Id.ToString());

        return Result.Success(document.Adapt<StaffDocumentResponse>());
    }
}
