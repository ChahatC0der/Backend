using SchoolERP.Application.Common.DTOs;

namespace SchoolERP.Application.Features.Student.DTOs;

// ==================== ENQUIRY ====================

public record CreateEnquiryRequest(
    string Source,
    string ParentName,
    string Mobile,
    string? StudentFirstName = null,
    string? StudentLastName = null,
    string? Email = null,
    long? ClassId = null,
    string? Message = null,
    long? AssignedTo = null
);

public record UpdateEnquiryRequest(
    long Id,
    string Source,
    string ParentName,
    string Mobile,
    string? StudentFirstName,
    string? StudentLastName,
    string? Email,
    long? ClassId,
    string? Message,
    long? AssignedTo
);

public record PatchEnquiryRequest(
    long Id,
    string? Source = null,
    string? ParentName = null,
    string? Mobile = null,
    string? StudentFirstName = null,
    string? StudentLastName = null,
    string? Email = null,
    long? ClassId = null,
    string? Message = null,
    long? AssignedTo = null
);

public record EnquiryResponse(
    long Id,
    Guid BranchId,
    DateTime EnquiryDate,
    string Source,
    string? StudentFirstName,
    string? StudentLastName,
    string ParentName,
    string Mobile,
    string? Email,
    long? ClassId,
    string? ClassName,
    string? Message,
    long? AssignedTo,
    string? AssignedToName,
    string Status,
    long? ConvertedToRegistrationId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record EnquiryLightResponse(
    long Id,
    string ParentName,
    string Mobile,
    string Status,
    DateTime EnquiryDate
);

// ==================== WORKFLOW REQUESTS ====================

public record AssignEnquiryRequest(
    long Id,
    long AssignedTo
);

public record ChangeEnquiryStatusRequest(
    long Id,
    string NewStatus,
    string? Reason = null
);

public record ConvertEnquiryToRegistrationRequest(
    long Id,
    long AcademicYearId,
    long ClassId,
    long? SectionId = null
);

// ==================== ENQUIRY FOLLOWUP ====================

public record CreateEnquiryFollowupRequest(
    long EnquiryId,
    string ActivityType,
    string? Notes = null,
    DateOnly? NextFollowupDate = null
);

public record UpdateEnquiryFollowupRequest(
    long Id,
    long EnquiryId,
    string ActivityType,
    string? Notes,
    DateOnly? NextFollowupDate
);

public record PatchEnquiryFollowupRequest(
    long Id,
    long? EnquiryId = null,
    string? ActivityType = null,
    string? Notes = null,
    DateOnly? NextFollowupDate = null
);

public record EnquiryFollowupResponse(
    long Id,
    long EnquiryId,
    string ActivityType,
    string? Notes,
    DateOnly? NextFollowupDate,
    long? CreatedBy,
    DateTime CreatedAt
);

public record EnquiryFollowupLightResponse(
    long Id,
    string ActivityType,
    DateOnly? NextFollowupDate,
    DateTime CreatedAt
);

// ==================== QUERY REQUESTS ====================

public record GetEnquiriesRequest : PagedRequest
{
    public string? Status { get; init; }
    public string? Source { get; init; }
    public long? ClassId { get; init; }
    public long? AssignedTo { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}
