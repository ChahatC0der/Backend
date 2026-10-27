namespace SchoolERP.Application.Features.Transport.DTOs.Route;

public sealed class PatchRouteStatusRequest
{
    public string Status { get; set; } = string.Empty;   // active | inactive
}
