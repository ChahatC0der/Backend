using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Alumni.Entities;

public class AlumniEvent : BranchEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string? Venue { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int? Capacity { get; set; }
    public bool IsRegistrationRequired { get; set; } = true;
    public decimal? Fee { get; set; }
    public long? CreatedBy { get; set; }

    public ICollection<AlumniEventRegistration> Registrations { get; set; } = new List<AlumniEventRegistration>();
}
