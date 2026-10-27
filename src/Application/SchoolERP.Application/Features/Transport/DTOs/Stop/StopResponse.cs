using StopEntity = SchoolERP.Domain.Transport.Entities.Stop;

namespace SchoolERP.Application.Features.Transport.DTOs.Stop;

public sealed class StopResponse
{
    public long Id { get; set; }
    public long RouteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StopOrder { get; set; }
    public TimeOnly? PickupTime { get; set; }
    public TimeOnly? DropTime { get; set; }
    public string? Landmark { get; set; }
    public decimal? DistanceFromCampus { get; set; }
    public string StopType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static StopResponse FromEntity(StopEntity s) => new()
    {
        Id = s.Id,
        RouteId = s.RouteId,
        Name = s.Name,
        StopOrder = s.StopOrder,
        PickupTime = s.PickupTime,
        DropTime = s.DropTime,
        Landmark = s.Landmark,
        DistanceFromCampus = s.DistanceFromCampus,
        StopType = s.StopType.ToString(),
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt,
    };
}
