using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;

namespace SchoolERP.Application.Features.Staff.Queries.Staff.GetStaffById;

public class GetStaffByIdQueryHandler : IRequestHandler<GetStaffByIdQuery, Result<StaffResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _currentBranchService;

    public GetStaffByIdQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _currentBranchService = branchService;
    }

    public async Task<Result<StaffResponse>> Handle(GetStaffByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _currentBranchService.GetBranchId() ?? Guid.Empty;

        var staff = await _dbContext.Set<StaffEntity>()
            .AsNoTracking()
            .Include(s => s.Department)
            .FirstOrDefaultAsync(s => s.Id == query.Id && s.BranchId == branchId && !s.IsDeleted, cancellationToken);

        if (staff == null)
            return Error.NotFound("Staff", query.Id.ToString());

        return Result.Success(staff.Adapt<StaffResponse>());
    }
}
