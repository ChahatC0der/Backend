using Mapster;
using SchoolERP.Application.Features.Student.DTOs;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;
using ParentEntity = SchoolERP.Domain.Student.Entities.Parent;
using StudentDocumentEntity = SchoolERP.Domain.Student.Entities.StudentDocument;
using SchoolERP.Domain.Student.Entities;

namespace SchoolERP.Application.Features.Student.Mappings;

public class StudentMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ==================== Student ====================
        config.NewConfig<CreateStudentRequest, StudentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.EnrollmentId)  // auto-generated
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Enrollments)
            .Ignore(dest => dest.Parents)
            .Ignore(dest => dest.Documents);

        config.NewConfig<UpdateStudentRequest, StudentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.EnrollmentId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Enrollments)
            .Ignore(dest => dest.Parents)
            .Ignore(dest => dest.Documents);

        config.NewConfig<PatchStudentRequest, StudentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.EnrollmentId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Enrollments)
            .Ignore(dest => dest.Parents)
            .Ignore(dest => dest.Documents);

        config.NewConfig<StudentEntity, StudentResponse>();

        config.NewConfig<StudentEntity, StudentLightResponse>()
            .Map(dest => dest.FullName,
                 src => (src.FirstName + " " + (src.LastName ?? "")).Trim());

        // ==================== StudentEnrollment ====================
        config.NewConfig<CreateStudentEnrollmentRequest, StudentEnrollmentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.IsCurrent)
            .Ignore(dest => dest.LeftAt)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Student);

        config.NewConfig<UpdateStudentEnrollmentRequest, StudentEnrollmentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.StudentId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Student);

        config.NewConfig<PatchStudentEnrollmentRequest, StudentEnrollmentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.StudentId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Student);

        config.NewConfig<StudentEnrollmentEntity, StudentEnrollmentResponse>()
            .Map(dest => dest.StudentName,
                 src => src.Student != null
                     ? (src.Student.FirstName + " " + (src.Student.LastName ?? "")).Trim()
                     : string.Empty);

        config.NewConfig<StudentEnrollmentEntity, StudentEnrollmentLightResponse>();

        // ==================== Parent ====================
        config.NewConfig<CreateParentRequest, ParentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Student);

        config.NewConfig<UpdateParentRequest, ParentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Student);

        config.NewConfig<PatchParentRequest, ParentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Student);

        config.NewConfig<ParentEntity, ParentResponse>();

        config.NewConfig<ParentEntity, ParentLightResponse>()
            .Map(dest => dest.FullName,
                 src => (src.FirstName + " " + (src.LastName ?? "")).Trim());

        // ==================== StudentDocument ====================
        config.NewConfig<CreateStudentDocumentRequest, StudentDocumentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Verified)      // default false
            .Ignore(dest => dest.UploadedBy)    // set by handler from current user
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Student);

        config.NewConfig<UpdateStudentDocumentRequest, StudentDocumentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Student);

        config.NewConfig<PatchStudentDocumentRequest, StudentDocumentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Student);

        config.NewConfig<StudentDocumentEntity, StudentDocumentResponse>();
        config.NewConfig<StudentDocumentEntity, StudentDocumentLightResponse>();
    }
}