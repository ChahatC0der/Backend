using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.TransportFee;
using SchoolERP.Domain.Shared.Results;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;
using StopEntity = SchoolERP.Domain.Transport.Entities.Stop;
using TransportFeeEntity = SchoolERP.Domain.Transport.Entities.TransportFee;

namespace SchoolERP.Application.Features.Transport.Queries.TransportFee.GetTransportFees;

public sealed class GetTransportFeesQueryHandler : IRequestHandler<GetTransportFeesQuery, Result<IReadOnlyList<TransportFeeResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetTransportFeesQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<IReadOnlyList<TransportFeeResponse>>> Handle(GetTransportFeesQuery request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var query = _db.Set<TransportFeeEntity>().AsNoTracking()
            .Where(f => f.BranchId == branchId && !f.IsDeleted);

        if (request.AcademicYearId.HasValue) query = query.Where(f => f.AcademicYearId == request.AcademicYearId.Value);
        if (request.RouteId.HasValue) query = query.Where(f => f.RouteId == request.RouteId.Value);
        if (request.StopId.HasValue) query = query.Where(f => f.StopId == request.StopId.Value);

        var rows = await query.OrderByDescending(f => f.CreatedAt).Take(500).ToListAsync(ct);

        // Enrich with names
        var routeIds = rows.Where(r => r.RouteId.HasValue).Select(r => r.RouteId!.Value).Distinct().ToList();
        var stopIds = rows.Where(r => r.StopId.HasValue).Select(r => r.StopId!.Value).Distinct().ToList();

        var routeLookup = await _db.Set<RouteEntity>().AsNoTracking()
            .Where(r => routeIds.Contains(r.Id))
            .ToDictionaryAsync(r => r.Id, r => new { r.Name, r.Code }, ct);
        var stopLookup = await _db.Set<StopEntity>().AsNoTracking()
            .Where(s => stopIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, ct);

        var result = rows.Select(f =>
        {
            string? routeName = f.RouteId.HasValue && routeLookup.TryGetValue(f.RouteId.Value, out var rn) ? rn.Name : null;
            string? routeCode = f.RouteId.HasValue && routeLookup.TryGetValue(f.RouteId.Value, out var rc) ? rc.Code : null;
            string? stopName = f.StopId.HasValue && stopLookup.TryGetValue(f.StopId.Value, out var sn) ? sn : null;
            return TransportFeeResponse.FromEntity(f, routeName, routeCode, stopName);
        }).ToList();

        return Result.Success<IReadOnlyList<TransportFeeResponse>>(result);
    }
}
