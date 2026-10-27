namespace SchoolERP.Application.Features.Transport.DTOs.Assignment;

/// <summary>Resolved transport fee for a student. Used by Fees Phase 6 integration.</summary>
public sealed class TransportAssignmentFeeQuoteResponse
{
    public long StudentId { get; set; }
    public long AcademicYearId { get; set; }
    public long AssignmentId { get; set; }

    public decimal Amount { get; set; }
    public string Frequency { get; set; } = string.Empty;   // monthly | quarterly | half_yearly
    public string SourceType { get; set; } = string.Empty;  // "route" | "stop"

    public long? RouteId { get; set; }
    public long? StopId { get; set; }

    public decimal PickupAmount { get; set; }
    public decimal DropAmount { get; set; }
    public bool UsedMaxPolicy { get; set; }                 // true when pickup != drop routes

    /// <summary>Fee head code from Fees module — always "TRANSPORT" in Phase 1.</summary>
    public string TransportFeeHeadCode { get; set; } = "TRANSPORT";
}
