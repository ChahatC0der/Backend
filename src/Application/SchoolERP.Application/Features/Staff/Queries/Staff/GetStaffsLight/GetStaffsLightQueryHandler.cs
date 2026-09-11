using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;

namespace SchoolERP.Application.Features.Staff.Queries.Staff.GetStaffsLight;

public class GetStaffsLightQueryHandler : IRequestHandler<GetStaffsLightQuery, Result<List<StaffLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetStaffsLightQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<List<StaffLightResponse>>> Handle(GetStaffsLightQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var items = await _dbContext.Set<StaffEntity>()
            .AsNoTracking()
            .Where(s => s.BranchId == branchId && !s.IsDeleted && s.Status == "active")
            .OrderBy(s => s.FirstName)
            .ProjectToType<StaffLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}
