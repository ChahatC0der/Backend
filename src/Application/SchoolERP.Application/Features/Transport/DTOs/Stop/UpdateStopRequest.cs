namespace SchoolERP.Application.Features.Transport.DTOs.Stop;

public sealed class UpdateStopRequest
{
    public string Name { get; set; } = string.Empty;
    public int StopOrder { get; set; }
    public TimeOnly? PickupTime { get; set; }
    public TimeOnly? DropTime { get; set; }
    public string? Landmark { get; set; }
    public decimal? DistanceFromCampus { get; set; }
    public string StopType { get; set; } = "both";
}
