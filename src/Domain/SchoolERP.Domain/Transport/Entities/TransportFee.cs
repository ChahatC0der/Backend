using SchoolERP.Domain.Common;
using SchoolERP.Domain.Transport.Enums;

namespace SchoolERP.Domain.Transport.Entities;

/// <summary>
/// Route OR Stop level transport fee. Exactly one of (RouteId, StopId) must be set — enforced in handler,
/// not DB (physical check permits both). Branch + academic-year scoped (no TenantId).
/// Frequency limited to monthly / quarterly / half_yearly per DB check constraint.
/// </summary>
public sealed class TransportFee : BranchEntity
{
    public long AcademicYearId { get; set; }
    public long? RouteId { get; set; }
    public long? StopId { get; set; }
    public decimal FeeAmount { get; set; }
    public TransportFeeFrequency Frequency { get; set; } = TransportFeeFrequency.Monthly;
}
