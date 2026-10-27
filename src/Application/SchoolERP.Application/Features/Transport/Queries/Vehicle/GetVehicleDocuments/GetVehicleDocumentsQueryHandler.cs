using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.VehicleDocument;
using SchoolERP.Domain.Shared.Results;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;
using VehicleDocumentEntity = SchoolERP.Domain.Transport.Entities.VehicleDocument;

namespace SchoolERP.Application.Features.Transport.Queries.Vehicle.GetVehicleDocuments;

public sealed class GetVehicleDocumentsQueryHandler : IRequestHandler<GetVehicleDocumentsQuery, Result<IReadOnlyList<VehicleDocumentResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetVehicleDocumentsQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<VehicleDocumentResponse>>> Handle(GetVehicleDocumentsQuery request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        // Verify vehicle belongs to scope
        var ok = await _db.Set<VehicleEntity>().AsNoTracking().AnyAsync(v =>
            v.Id == request.VehicleId && v.TenantId == tenantId
            && v.BranchId == branchId && !v.IsDeleted, ct);
        if (!ok) return Error.NotFound("Vehicle", request.VehicleId.ToString());

        var rows = await _db.Set<VehicleDocumentEntity>().AsNoTracking()
            .Where(d => d.VehicleId == request.VehicleId && !d.IsDeleted)
            .OrderBy(d => d.DocumentType)
            .ToListAsync(ct);

        return Result.Success<IReadOnlyList<VehicleDocumentResponse>>(
            rows.Select(VehicleDocumentResponse.FromEntity).ToList());
    }
}
