namespace SchoolERP.Application.Features.Transport.DTOs.Route;

public sealed class RouteLightResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
