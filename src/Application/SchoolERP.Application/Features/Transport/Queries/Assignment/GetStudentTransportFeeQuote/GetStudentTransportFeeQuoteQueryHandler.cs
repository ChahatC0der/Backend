using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.Assignment;
using SchoolERP.Domain.Shared.Results;
using TransportAssignmentEntity = SchoolERP.Domain.Transport.Entities.TransportAssignment;
using TransportFeeEntity = SchoolERP.Domain.Transport.Entities.TransportFee;

namespace SchoolERP.Application.Features.Transport.Queries.Assignment.GetStudentTransportFeeQuote;

/// <summary>
/// Resolves effective transport fee: stop-fee precedence over route-fee; max(pickup, drop) for split routes.
/// Returns null when no assignment or no fee configured.
/// </summary>
public sealed class GetStudentTransportFeeQuoteQueryHandler : IRequestHandler<GetStudentTransportFeeQuoteQuery, Result<TransportAssignmentFeeQuoteResponse?>>
{
    private readonly SchoolERP.Application.Common.Interfaces.IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public GetStudentTransportFeeQuoteQueryHandler(SchoolERP.Application.Common.Interfaces.IApplicationDbContext db, ICurrentTenantService tenant)
    { _db = db; _tenant = tenant; }

    public async Task<Result<TransportAssignmentFeeQuoteResponse?>> Handle(GetStudentTransportFeeQuoteQuery request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");

        var assignment = await _db.Set<TransportAssignmentEntity>().AsNoTracking()
            .FirstOrDefaultAsync(a => a.TenantId == tenantId && a.StudentId == request.StudentId
                                   && a.AcademicYearId == request.AcademicYearId && a.IsCurrent && !a.IsDeleted, ct);
        if (assignment is null) return Result.Success<TransportAssignmentFeeQuoteResponse?>((TransportAssignmentFeeQuoteResponse?)null);

        var pickup = await ResolveFeeAsync(assignment.PickupStopId, assignment.PickupRouteId, request.AcademicYearId, ct);

        decimal dropAmount = 0;
        var dropRouteId = assignment.DropRouteId ?? assignment.PickupRouteId;
        var dropStopId = assignment.DropStopId ?? assignment.PickupStopId;
        if (dropRouteId != assignment.PickupRouteId || dropStopId != assignment.PickupStopId)
            dropAmount = (await ResolveFeeAsync(dropStopId, dropRouteId, request.AcademicYearId, ct)).Amount;

        if (pickup.Amount == 0 && dropAmount == 0)
            return Result.Success<TransportAssignmentFeeQuoteResponse?>((TransportAssignmentFeeQuoteResponse?)null);

        var usedMax = dropAmount > pickup.Amount;
        var finalAmount = Math.Max(pickup.Amount, dropAmount);

        return Result.Success<TransportAssignmentFeeQuoteResponse?>(new TransportAssignmentFeeQuoteResponse
        {
            StudentId = request.StudentId,
            AcademicYearId = request.AcademicYearId,
            AssignmentId = assignment.Id,
            Amount = finalAmount,
            Frequency = pickup.Frequency ?? "monthly",
            SourceType = usedMax ? "drop" : "pickup",
            RouteId = usedMax ? dropRouteId : assignment.PickupRouteId,
            StopId = usedMax ? dropStopId : assignment.PickupStopId,
            PickupAmount = pickup.Amount,
            DropAmount = dropAmount,
            UsedMaxPolicy = usedMax,
            TransportFeeHeadCode = "TRANSPORT",
        });
    }

    private sealed record FeeResult(decimal Amount, string? Frequency);

    private async Task<FeeResult> ResolveFeeAsync(long stopId, long routeId, long academicYearId, CancellationToken ct)
    {
        var stopFee = await _db.Set<TransportFeeEntity>().AsNoTracking()
            .Where(f => f.AcademicYearId == academicYearId && f.StopId == stopId && !f.IsDeleted)
            .Select(f => new { f.FeeAmount, Freq = f.Frequency.ToString() })
            .FirstOrDefaultAsync(ct);
        if (stopFee is not null) return new FeeResult(stopFee.FeeAmount, stopFee.Freq);

        var routeFee = await _db.Set<TransportFeeEntity>().AsNoTracking()
            .Where(f => f.AcademicYearId == academicYearId && f.RouteId == routeId && !f.IsDeleted)
            .Select(f => new { f.FeeAmount, Freq = f.Frequency.ToString() })
            .FirstOrDefaultAsync(ct);

        return routeFee is not null ? new FeeResult(routeFee.FeeAmount, routeFee.Freq) : new FeeResult(0, null);
    }
}
