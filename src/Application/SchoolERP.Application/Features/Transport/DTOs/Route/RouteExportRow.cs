namespace SchoolERP.Application.Features.Transport.DTOs.Route;

public sealed class RouteExportRow
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int StopCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
