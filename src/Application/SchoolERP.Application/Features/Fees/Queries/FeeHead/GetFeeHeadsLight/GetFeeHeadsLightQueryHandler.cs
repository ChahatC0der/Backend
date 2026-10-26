using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.FeeHead;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.FeeHead.GetFeeHeadsLight;

public sealed class GetFeeHeadsLightQueryHandler : IRequestHandler<GetFeeHeadsLightQuery, Result<IReadOnlyList<FeeHeadLightResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetFeeHeadsLightQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<IReadOnlyList<FeeHeadLightResponse>>> Handle(GetFeeHeadsLightQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var rows = await _db.Set<FeeHeadEntity>().AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.BranchId == branchId &&
                        x.DeletedAt == null && x.Status == FeeHeadStatus.Active)
            .OrderBy(x => x.Name)
            .Select(x => new FeeHeadLightResponse { Id = x.Id, Name = x.Name, Code = x.Code })
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<FeeHeadLightResponse>>(rows);
    }
}
