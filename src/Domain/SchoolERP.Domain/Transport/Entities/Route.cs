using SchoolERP.Domain.Common;
using SchoolERP.Domain.Transport.Enums;

namespace SchoolERP.Domain.Transport.Entities;

/// <summary>Branch-scoped route. Code unique per branch. No TenantId (validate branch.TenantId in handlers).</summary>
public sealed class Route : BranchEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RouteStatus Status { get; set; } = RouteStatus.Active;

    public ICollection<Stop> Stops { get; set; } = new List<Stop>();
}
