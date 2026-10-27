namespace SchoolERP.Application.Features.Transport.DTOs.Route;

public sealed class CreateRouteRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
}
