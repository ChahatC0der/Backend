namespace SchoolERP.Application.Features.Transport.DTOs.Stop;

/// <summary>Order array = final StopOrder (index+1). All stops in route must be present.</summary>
public sealed class ReorderStopsRequest
{
    public List<long> StopIds { get; set; } = new();
}
