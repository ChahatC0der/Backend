using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Alumni.Entities;

public class AlumniSuccessStory : BaseEntity
{
    public long AlumniId { get; set; }
    public string Story { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string? VideoUrl { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public long? ApprovedBy { get; set; }
    public long? CreatedBy { get; set; }

    public Alumni Alumni { get; set; } = null!;
}
