using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Fees.DTOs.Concession;
using SchoolERP.Domain.Shared.Results;
using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;

namespace SchoolERP.Application.Features.Fees.Queries.Concession.GetConcessionLights;

public sealed class GetConcessionLightsQueryHandler
    : IRequestHandler<GetConcessionLightsQuery, Result<List<ConcessionLightResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetConcessionLightsQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<List<ConcessionLightResponse>>> Handle(GetConcessionLightsQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var rows = await _db.Set<ConcessionEntity>().AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.BranchId == branchId && x.DeletedAt == null)
            .OrderBy(x => x.Name)
            .Select(x => new ConcessionLightResponse
            {
                Id = x.Id,
                Name = x.Name
                
            })
            .ToListAsync(ct);

        return Result.Success(rows);
    }
}