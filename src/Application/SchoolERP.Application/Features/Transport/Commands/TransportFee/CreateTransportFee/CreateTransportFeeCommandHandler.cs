using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.DTOs.TransportFee;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Transport.Enums;
using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;
using StopEntity = SchoolERP.Domain.Transport.Entities.Stop;
using TransportFeeEntity = SchoolERP.Domain.Transport.Entities.TransportFee;

namespace SchoolERP.Application.Features.Transport.Commands.TransportFee.CreateTransportFee;

public sealed class CreateTransportFeeCommandHandler : IRequestHandler<CreateTransportFeeCommand, Result<TransportFeeResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentAcademicYearProvider _ay;

    public CreateTransportFeeCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant,
        ICurrentBranchService branch, ICurrentAcademicYearProvider ay)
    { _db = db; _tenant = tenant; _branch = branch; _ay = ay; }

    public async Task<Result<TransportFeeResponse>> Handle(CreateTransportFeeCommand request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        // AY must exist
        var ay = await _ay.GetByIdAsync(request.Request.AcademicYearId, ct);
        if (ay is null) return Error.NotFound("AcademicYear", request.Request.AcademicYearId.ToString());

        if (!TryParseFrequency(request.Request.Frequency, out var freq))
            return Error.Validation($"Invalid Frequency: {request.Request.Frequency}.");

        // Route or Stop validation + scope
        string? routeName = null, routeCode = null, stopName = null;

        if (request.Request.RouteId.HasValue)
        {
            var route = await _db.Set<RouteEntity>().AsNoTracking().FirstOrDefaultAsync(r =>
                r.Id == request.Request.RouteId.Value && r.BranchId == branchId && !r.IsDeleted, ct);
            if (route is null) return Error.NotFound("Route", request.Request.RouteId.Value.ToString());
            routeName = route.Name; routeCode = route.Code;

            // Duplicate check: same (AY, RouteId)
            var dup = await _db.Set<TransportFeeEntity>().AnyAsync(f =>
                f.BranchId == branchId && f.AcademicYearId == request.Request.AcademicYearId
                && !f.IsDeleted && f.RouteId == request.Request.RouteId, ct);
            if (dup) return Error.Conflict($"A fee is already configured for this route and academic year. Use update to modify.");
        }
        else
        {
            var stop = await _db.Set<StopEntity>().AsNoTracking().FirstOrDefaultAsync(s =>
                s.Id == request.Request.StopId!.Value && !s.IsDeleted, ct);
            if (stop is null) return Error.NotFound("Stop", request.Request.StopId.Value.ToString());

            // Verify stop belongs to a route in this branch
            var stopRouteOk = await _db.Set<RouteEntity>().AsNoTracking().AnyAsync(r =>
                r.Id == stop.RouteId && r.BranchId == branchId && !r.IsDeleted, ct);
            if (!stopRouteOk) return Error.NotFound("Stop", request.Request.StopId.Value.ToString());
            stopName = stop.Name;

            var dup = await _db.Set<TransportFeeEntity>().AnyAsync(f =>
                f.BranchId == branchId && f.AcademicYearId == request.Request.AcademicYearId
                && !f.IsDeleted && f.StopId == request.Request.StopId, ct);
            if (dup) return Error.Conflict($"A fee is already configured for this stop and academic year. Use update to modify.");
        }

        var entity = new TransportFeeEntity
        {
            BranchId = branchId,
            AcademicYearId = request.Request.AcademicYearId,
            RouteId = request.Request.RouteId,
            StopId = request.Request.StopId,
            FeeAmount = request.Request.FeeAmount,
            Frequency = freq,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Set<TransportFeeEntity>().Add(entity);
        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("TransportFees", entity.Id, FinancialAuditAction.Create, null, entity, ct);

        return Result.Success(TransportFeeResponse.FromEntity(entity, routeName, routeCode, stopName));
    }

    internal static bool TryParseFrequency(string value, out TransportFeeFrequency freq)
    {
        foreach (var kv in EnumMaps_JsonAdapter.TransportFeeFrequencyMap)
            if (string.Equals(kv.Value, value, StringComparison.OrdinalIgnoreCase)) { freq = kv.Key; return true; }
        freq = default; return false;
    }
}
