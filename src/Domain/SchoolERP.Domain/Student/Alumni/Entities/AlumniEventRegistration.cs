using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Alumni.Entities;

public class AlumniEventRegistration : BaseEntity
{
    public long EventId { get; set; }
    public long AlumniId { get; set; }
    public string Status { get; set; } = "registered";
    public bool FeePaid { get; set; }
    public decimal? FeeAmount { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public AlumniEvent Event { get; set; } = null!;
    public Alumni Alumni { get; set; } = null!;
}
