using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Admission.Entities;

public class Admission : BranchEntity
{
    public Guid TenantId { get; set; }
    public long AcademicYearId { get; set; }

    // Source links
    public long? RegistrationId { get; set; }
    public long? EnquiryId { get; set; }
    public long? StudentId { get; set; }

    // Academic placement
    public long ClassId { get; set; }
    public long SectionId { get; set; }

    // Admission data
    public DateOnly AdmissionDate { get; set; }
    public string? AdmissionNumber { get; set; }
    public string? RollNumber { get; set; }
    public DateOnly? JoiningDate { get; set; }
    public string? PreviousSchool { get; set; }

    // First fee
    public bool FirstFeePaid { get; set; }
    public decimal? FirstFeeAmount { get; set; }
    public DateOnly? FirstFeePaymentDate { get; set; }
    public string? FirstFeePaymentMode { get; set; }

    // Transport
    public long? TransportRouteId { get; set; }

    // Enrollment
    public DateTime? EnrolledAt { get; set; }
    public long? EnrolledBy { get; set; }

    // Created by
    public long? CreatedBy { get; set; }

    // Status
    public string Status { get; set; } = "in_progress";

    // Navigation
    public ICollection<AdmissionAuditLog> AuditLogs { get; set; } = new List<AdmissionAuditLog>();
}
