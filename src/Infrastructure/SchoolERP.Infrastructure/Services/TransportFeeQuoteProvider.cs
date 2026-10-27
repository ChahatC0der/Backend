using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Infrastructure.Persistence;
using TransportAssignmentEntity = SchoolERP.Domain.Transport.Entities.TransportAssignment;
using TransportFeeEntity = SchoolERP.Domain.Transport.Entities.TransportFee;

namespace SchoolERP.Infrastructure.Services;

/// <summary>
/// Reads active transport assignment + configured fee, returns effective amount.
/// Logic mirrors GetStudentTransportFeeQuoteQueryHandler but callable from Fees namespace.
/// </summary>
public sealed class TransportFeeQuoteProvider : ITransportFeeQuoteProvider
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public TransportFeeQuoteProvider(AppDbContext db, ICurrentTenantService tenant)
    { _db = db; _tenant = tenant; }

    public async Task<TransportFeeQuote?> GetQuoteAsync(long studentId, long academicYearId, CancellationToken ct = default)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return null;

        var assignment = await _db.Set<TransportAssignmentEntity>().AsNoTracking()
            .FirstOrDefaultAsync(a => a.TenantId == tenantId
                && a.StudentId == studentId
                && a.AcademicYearId == academicYearId
                && a.IsCurrent && !a.IsDeleted, ct);
        if (assignment is null) return null;

        var pickup = await ResolveFeeAsync(assignment.PickupStopId, assignment.PickupRouteId, academicYearId, ct);

        decimal dropAmount = 0;
        var dropRouteId = assignment.DropRouteId ?? assignment.PickupRouteId;
        var dropStopId = assignment.DropStopId ?? assignment.PickupStopId;
        if (dropRouteId != assignment.PickupRouteId || dropStopId != assignment.PickupStopId)
            dropAmount = (await ResolveFeeAsync(dropStopId, dropRouteId, academicYearId, ct)).Amount;

        if (pickup.Amount == 0 && dropAmount == 0) return null;

        var usedMax = dropAmount > pickup.Amount;
        return new TransportFeeQuote(
            Amount: Math.Max(pickup.Amount, dropAmount),
            Frequency: pickup.Frequency ?? "monthly",
            RouteId: usedMax ? dropRouteId : assignment.PickupRouteId,
            StopId: usedMax ? dropStopId : assignment.PickupStopId,
            TransportFeeHeadCode: "TRANSPORT");
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
