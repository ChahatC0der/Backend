using TransportFeeEntity = SchoolERP.Domain.Transport.Entities.TransportFee;

namespace SchoolERP.Application.Features.Transport.DTOs.TransportFee;

public sealed class TransportFeeResponse
{
    public long Id { get; set; }
    public long AcademicYearId { get; set; }
    public long? RouteId { get; set; }
    public string? RouteName { get; set; }
    public string? RouteCode { get; set; }
    public long? StopId { get; set; }
    public string? StopName { get; set; }
    public decimal FeeAmount { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;   // "route" | "stop"
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static TransportFeeResponse FromEntity(
        TransportFeeEntity f,
        string? routeName = null,
        string? routeCode = null,
        string? stopName = null) => new()
    {
        Id = f.Id,
        AcademicYearId = f.AcademicYearId,
        RouteId = f.RouteId,
        RouteName = routeName,
        RouteCode = routeCode,
        StopId = f.StopId,
        StopName = stopName,
        FeeAmount = f.FeeAmount,
        Frequency = f.Frequency.ToString(),
        SourceType = f.StopId.HasValue ? "stop" : "route",
        CreatedAt = f.CreatedAt,
        UpdatedAt = f.UpdatedAt,
    };
}
