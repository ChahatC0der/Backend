using Mapster;
using SchoolERP.Application.Features.Student.DTOs;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;
using RegistrationDocumentEntity = SchoolERP.Domain.Student.Registration.Entities.RegistrationDocument;

namespace SchoolERP.Application.Features.Student.Mappings;

public class RegistrationMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ==================== Registration ====================
        config.NewConfig<CreateRegistrationRequest, RegistrationEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.RegistrationNumber)
            .Ignore(dest => dest.RegistrationFeePaid)
            .Ignore(dest => dest.RegistrationFeeAmount)
            .Ignore(dest => dest.RegistrationFeePaymentDate)
            .Ignore(dest => dest.RegistrationFeePaymentMode)
            .Ignore(dest => dest.InterviewDate)
            .Ignore(dest => dest.InterviewMode)
            .Ignore(dest => dest.InterviewerId)
            .Ignore(dest => dest.InterviewResult)
            .Ignore(dest => dest.InterviewFeedback)
            .Ignore(dest => dest.ApprovedAt)
            .Ignore(dest => dest.ApprovedBy)
            .Ignore(dest => dest.RejectionReason)
            .Ignore(dest => dest.ParentUserId)
            .Ignore(dest => dest.ConvertedToStudentId)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Documents)
            .Ignore(dest => dest.AuditLogs);

        config.NewConfig<UpdateRegistrationRequest, RegistrationEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.RegistrationNumber)
            .Ignore(dest => dest.EnquiryId)
            .Ignore(dest => dest.RegistrationFeePaid)
            .Ignore(dest => dest.RegistrationFeeAmount)
            .Ignore(dest => dest.RegistrationFeePaymentDate)
            .Ignore(dest => dest.RegistrationFeePaymentMode)
            .Ignore(dest => dest.InterviewDate)
            .Ignore(dest => dest.InterviewMode)
            .Ignore(dest => dest.InterviewerId)
            .Ignore(dest => dest.InterviewResult)
            .Ignore(dest => dest.InterviewFeedback)
            .Ignore(dest => dest.ApprovedAt)
            .Ignore(dest => dest.ApprovedBy)
            .Ignore(dest => dest.RejectionReason)
            .Ignore(dest => dest.ParentUserId)
            .Ignore(dest => dest.ConvertedToStudentId)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Documents)
            .Ignore(dest => dest.AuditLogs);

        config.NewConfig<PatchRegistrationRequest, RegistrationEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.RegistrationNumber)
            .Ignore(dest => dest.EnquiryId)
            .Ignore(dest => dest.RegistrationFeePaid)
            .Ignore(dest => dest.RegistrationFeeAmount)
            .Ignore(dest => dest.RegistrationFeePaymentDate)
            .Ignore(dest => dest.RegistrationFeePaymentMode)
            .Ignore(dest => dest.InterviewDate)
            .Ignore(dest => dest.InterviewMode)
            .Ignore(dest => dest.InterviewerId)
            .Ignore(dest => dest.InterviewResult)
            .Ignore(dest => dest.InterviewFeedback)
            .Ignore(dest => dest.ApprovedAt)
            .Ignore(dest => dest.ApprovedBy)
            .Ignore(dest => dest.RejectionReason)
            .Ignore(dest => dest.ParentUserId)
            .Ignore(dest => dest.ConvertedToStudentId)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Documents)
            .Ignore(dest => dest.AuditLogs);

        // Registration -> Response (computed fields filled in handler)
        config.NewConfig<RegistrationEntity, RegistrationResponse>()
            .Map(dest => dest.ClassName, src => (string?)null)
            .Map(dest => dest.SectionName, src => (string?)null)
            .Map(dest => dest.InterviewerName, src => (string?)null)
            .Map(dest => dest.ApprovedByName, src => (string?)null);

        config.NewConfig<RegistrationEntity, RegistrationLightResponse>()
            .Map(dest => dest.StudentFullName,
                src => (src.StudentFirstName + " " + (src.StudentLastName ?? "")).Trim());

        // ==================== RegistrationDocument ====================
        config.NewConfig<CreateRegistrationDocumentRequest, RegistrationDocumentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.RejectionReason)
            .Ignore(dest => dest.VerifiedBy)
            .Ignore(dest => dest.VerifiedAt)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Registration);

        config.NewConfig<UpdateRegistrationDocumentRequest, RegistrationDocumentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.RegistrationId)
            .Ignore(dest => dest.VerifiedBy)
            .Ignore(dest => dest.VerifiedAt)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Registration);

        config.NewConfig<PatchRegistrationDocumentRequest, RegistrationDocumentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.RegistrationId)
            .Ignore(dest => dest.VerifiedBy)
            .Ignore(dest => dest.VerifiedAt)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Registration);

        config.NewConfig<RegistrationDocumentEntity, RegistrationDocumentResponse>();
        config.NewConfig<RegistrationDocumentEntity, RegistrationDocumentLightResponse>();
    }
}
