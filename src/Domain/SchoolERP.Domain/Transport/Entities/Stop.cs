using SchoolERP.Domain.Common;
using SchoolERP.Domain.Transport.Enums;

namespace SchoolERP.Domain.Transport.Entities;

/// <summary>Stop scoped through Route. StopOrder unique per route. No direct tenant/branch — validate via parent Route.</summary>
public sealed class Stop : BaseEntity
{
    public long RouteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StopOrder { get; set; }
    public TimeOnly? PickupTime { get; set; }
    public TimeOnly? DropTime { get; set; }
    public string? Landmark { get; set; }
    public decimal? DistanceFromCampus { get; set; }
    public StopType StopType { get; set; } = StopType.Both;
}
