using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.Vehicle;
using SchoolERP.Domain.Shared.Results;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;

namespace SchoolERP.Application.Features.Transport.Queries.Vehicle.GetVehicleById;

public sealed class GetVehicleByIdQueryHandler : IRequestHandler<GetVehicleByIdQuery, Result<VehicleResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetVehicleByIdQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<VehicleResponse>> Handle(GetVehicleByIdQuery request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var entity = await _db.Set<VehicleEntity>().AsNoTracking().FirstOrDefaultAsync(v =>
            v.Id == request.Id && v.TenantId == tenantId
            && v.BranchId == branchId && !v.IsDeleted, ct);
        if (entity is null) return Error.NotFound("Vehicle", request.Id.ToString());

        return Result.Success(VehicleResponse.FromEntity(entity));
    }
}
