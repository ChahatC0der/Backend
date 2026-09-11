using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Entities;

public class StudentEnrollment : BaseEntity
{
    public long StudentId { get; set; }
    public long AcademicYearId { get; set; }
    public long ClassId { get; set; }
    public long SectionId { get; set; }

    public string? RollNumber { get; set; }
    public bool IsCurrent { get; set; } = true;
    public DateOnly EnrolledAt { get; set; }
    public DateOnly? LeftAt { get; set; }

    // Navigation
    public Student Student { get; set; } = null!;
}
