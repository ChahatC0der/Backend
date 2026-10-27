namespace SchoolERP.Application.Features.Transport.DTOs.TransportFee;

/// <summary>RouteId / StopId cannot change after creation — update only amount and frequency.</summary>
public sealed class UpdateTransportFeeRequest
{
    public decimal FeeAmount { get; set; }
    public string Frequency { get; set; } = "monthly";
}
