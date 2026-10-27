namespace SchoolERP.Application.Features.Transport.DTOs.TransportFee;

/// <summary>Exactly one of RouteId or StopId must be provided.</summary>
public sealed class CreateTransportFeeRequest
{
    public long AcademicYearId { get; set; }
    public long? RouteId { get; set; }
    public long? StopId { get; set; }
    public decimal FeeAmount { get; set; }
    public string Frequency { get; set; } = "monthly";   // monthly | quarterly | half_yearly
}
