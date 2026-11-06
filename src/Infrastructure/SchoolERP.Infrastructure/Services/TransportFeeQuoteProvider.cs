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


    /// <summary>
    /// Resolves the effective transport charge for a student's
    /// active assignment.
    ///
    /// When separate pickup and drop configurations exist,
    /// the higher configured amount is selected.
    ///
    /// Important:
    /// Amount, frequency, route and stop must all come from
    /// the same selected configuration.
    /// </summary>
    public async Task<TransportFeeQuote?> GetQuoteAsync(
        long studentId,
        long academicYearId,
        CancellationToken ct = default)
    {
        // BLOCK 1: Resolve current tenant scope.
        var tenantId = _tenant.GetTenantId();

        if (tenantId == Guid.Empty)
        {
            return null;
        }

        // BLOCK 2: Find the student's active transport assignment.
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
        {
            return null;
        }

        // BLOCK 3: Resolve pickup stop/route fee configuration.
        var pickup = await ResolveFeeAsync(
            assignment.PickupStopId,
            assignment.PickupRouteId,
            academicYearId,
            ct);

        // BLOCK 4: Resolve drop configuration separately.
        // When drop route and stop are identical to pickup,
        // do not perform another database lookup.
        var dropRouteId = assignment.DropRouteId
            ?? assignment.PickupRouteId;

        var dropStopId = assignment.DropStopId
            ?? assignment.PickupStopId;

        var drop = new FeeResult(0m, null);

        if (dropRouteId != assignment.PickupRouteId ||
            dropStopId != assignment.PickupStopId)
        {
            drop = await ResolveFeeAsync(
                dropStopId,
                dropRouteId,
                academicYearId,
                ct);
        }

        // BLOCK 5: Return null when neither configuration
        // contains a charge.
        if (pickup.Amount <= 0m && drop.Amount <= 0m)
        {
            return null;
        }

        // BLOCK 6: Select one complete fee configuration.
        // Do not select the amount from one configuration
        // and the frequency from another.
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

        // BLOCK 7: Return the effective transport quote.
        return new TransportFeeQuote(
            Amount: selectedFee.Amount,
            Frequency: selectedFee.Frequency ?? "monthly",
            RouteId: selectedRouteId,
            StopId: selectedStopId,
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
