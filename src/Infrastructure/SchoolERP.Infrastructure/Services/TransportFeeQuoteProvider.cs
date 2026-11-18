
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Infrastructure.Persistence;
using TransportAssignmentEntity = SchoolERP.Domain.Transport.Entities.TransportAssignment;
using TransportFeeEntity = SchoolERP.Domain.Transport.Entities.TransportFee;

namespace SchoolERP.Infrastructure.Services;

/// <summary>
/// Provides the effective transport fee for a student's
/// current transport assignment.
/// </summary>
public sealed class TransportFeeQuoteProvider : ITransportFeeQuoteProvider
{
    private readonly AppDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public TransportFeeQuoteProvider(
        AppDbContext db,
        ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<TransportFeeQuote?> GetQuoteAsync(
        long studentId,
        long academicYearId,
        CancellationToken ct = default)
    {
        // BLOCK 1: Validate tenant context.
        var tenantId = _tenant.GetTenantId();

        if (tenantId == Guid.Empty)
            return null;

        // BLOCK 2: Find current student transport assignment.
        var assignment = await _db.Set<TransportAssignmentEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(a =>
                a.TenantId == tenantId &&
                a.StudentId == studentId &&
                a.AcademicYearId == academicYearId &&
                a.IsCurrent &&
                !a.IsDeleted,
                ct);

        if (assignment is null)
            return null;

        // BLOCK 3: Resolve pickup fee.
        var pickup = await ResolveFeeAsync(
            assignment.PickupStopId,
            assignment.PickupRouteId,
            academicYearId,
            ct);

        // BLOCK 4: Resolve effective drop location.
        // Null drop values mean same as pickup.
        var dropRouteId = assignment.DropRouteId
            ?? assignment.PickupRouteId;

        var dropStopId = assignment.DropStopId
            ?? assignment.PickupStopId;

        // BLOCK 5: Same pickup and drop location means
        // only one transport fee is applicable.
        if (dropRouteId == assignment.PickupRouteId &&
            dropStopId == assignment.PickupStopId)
        {
            return CreateQuote(
                pickup,
                assignment.PickupRouteId,
                assignment.PickupStopId);
        }

        // BLOCK 6: Different pickup/drop locations.
        var drop = await ResolveFeeAsync(
            dropStopId,
            dropRouteId,
            academicYearId,
            ct);

        // BLOCK 7: No configured fee found.
        if (pickup.Amount <= 0m && drop.Amount <= 0m)
            return null;

        // BLOCK 8: Apply max-fee policy.
        // Never add pickup and drop charges.
        // Amount and frequency must come from the same configuration.
        var useDropConfiguration = drop.Amount > pickup.Amount;

        var selectedFee = useDropConfiguration
            ? drop
            : pickup;

        var selectedRouteId = useDropConfiguration
            ? dropRouteId
            : assignment.PickupRouteId;

        var selectedStopId = useDropConfiguration
            ? dropStopId
            : assignment.PickupStopId;

        // BLOCK 9: Return selected fee configuration.
        return CreateQuote(
            selectedFee,
            selectedRouteId,
            selectedStopId);
    }

    private sealed record FeeResult(
        decimal Amount,
        string? Frequency);

    private static TransportFeeQuote? CreateQuote(
        FeeResult fee,
        long routeId,
        long stopId)
    {
        if (fee.Amount <= 0m)
            return null;

        return new TransportFeeQuote(
            Amount: fee.Amount,
            Frequency: fee.Frequency ?? "monthly",
            RouteId: routeId,
            StopId: stopId,
            TransportFeeHeadCode: "TRANSPORT");
    }

    private async Task<FeeResult> ResolveFeeAsync(
        long stopId,
        long routeId,
        long academicYearId,
        CancellationToken ct)
    {
        // Stop-level fee has priority over route-level fee.
        var stopFee = await _db.Set<TransportFeeEntity>()
            .AsNoTracking()
            .Where(f =>
                f.AcademicYearId == academicYearId &&
                f.StopId == stopId &&
                !f.IsDeleted)
            .Select(f => new
            {
                f.FeeAmount,
                Frequency = f.Frequency.ToString()
            })
            .FirstOrDefaultAsync(ct);

        if (stopFee is not null)
        {
            return new FeeResult(
                stopFee.FeeAmount,
                stopFee.Frequency);
        }

        // Fallback to route-level fee.
        var routeFee = await _db.Set<TransportFeeEntity>()
            .AsNoTracking()
            .Where(f =>
                f.AcademicYearId == academicYearId &&
                f.RouteId == routeId &&
                !f.IsDeleted)
            .Select(f => new
            {
                f.FeeAmount,
                Frequency = f.Frequency.ToString()
            })
            .FirstOrDefaultAsync(ct);

        return routeFee is not null
            ? new FeeResult(routeFee.FeeAmount, routeFee.Frequency)
            : new FeeResult(0m, null);
    }
}
