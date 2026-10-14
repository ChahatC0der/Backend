using Mapster;
using SchoolERP.Application.Features.Student.DTOs;
using AdmissionEntity = SchoolERP.Domain.Student.Admission.Entities.Admission;

namespace SchoolERP.Application.Features.Student.Mappings;

public class AdmissionMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ==================== Admission ====================
        config.NewConfig<CreateAdmissionRequest, AdmissionEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.StudentId)
            .Ignore(dest => dest.FirstFeePaid)
            .Ignore(dest => dest.FirstFeeAmount)
            .Ignore(dest => dest.FirstFeePaymentDate)
            .Ignore(dest => dest.FirstFeePaymentMode)
            .Ignore(dest => dest.EnrolledAt)
            .Ignore(dest => dest.EnrolledBy)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.AuditLogs);

        config.NewConfig<UpdateAdmissionRequest, AdmissionEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.RegistrationId)
            .Ignore(dest => dest.EnquiryId)
            .Ignore(dest => dest.StudentId)
            .Ignore(dest => dest.FirstFeePaid)
            .Ignore(dest => dest.FirstFeeAmount)
            .Ignore(dest => dest.FirstFeePaymentDate)
            .Ignore(dest => dest.FirstFeePaymentMode)
            .Ignore(dest => dest.EnrolledAt)
            .Ignore(dest => dest.EnrolledBy)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.AuditLogs);

        config.NewConfig<PatchAdmissionRequest, AdmissionEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.RegistrationId)
            .Ignore(dest => dest.EnquiryId)
            .Ignore(dest => dest.StudentId)
            .Ignore(dest => dest.FirstFeePaid)
            .Ignore(dest => dest.FirstFeeAmount)
            .Ignore(dest => dest.FirstFeePaymentDate)
            .Ignore(dest => dest.FirstFeePaymentMode)
            .Ignore(dest => dest.EnrolledAt)
            .Ignore(dest => dest.EnrolledBy)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.Status)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.AuditLogs);

        // Admission -> Response (computed fields filled in handler)
        config.NewConfig<AdmissionEntity, AdmissionResponse>()
            .Map(dest => dest.AcademicYearName, src => (string?)null)
            .Map(dest => dest.RegistrationNumber, src => (string?)null)
            .Map(dest => dest.StudentName, src => (string?)null)
            .Map(dest => dest.EnrollmentId, src => (string?)null)
            .Map(dest => dest.ClassName, src => (string?)null)
            .Map(dest => dest.SectionName, src => (string?)null)
            .Map(dest => dest.EnrolledByName, src => (string?)null);

        config.NewConfig<AdmissionEntity, AdmissionLightResponse>()
            .Map(dest => dest.StudentName, src => (string?)null)
            .Map(dest => dest.EnrollmentId, src => (string?)null)
            .Map(dest => dest.RegistrationNumber, src => (string?)null);
    }
}
