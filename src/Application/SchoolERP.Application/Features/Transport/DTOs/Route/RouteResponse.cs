using RouteEntity = SchoolERP.Domain.Transport.Entities.Route;

namespace SchoolERP.Application.Features.Transport.DTOs.Route;

public sealed class RouteResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int StopCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static RouteResponse FromEntity(RouteEntity r, int stopCount = 0) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Code = r.Code,
        Description = r.Description,
        Status = r.Status.ToString(),
        StopCount = stopCount,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
    };
}
