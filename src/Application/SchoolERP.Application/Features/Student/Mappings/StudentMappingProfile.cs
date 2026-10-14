using Mapster;
using SchoolERP.Application.Features.Student.DTOs;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;
using ParentEntity = SchoolERP.Domain.Student.Entities.Parent;
using StudentDocumentEntity = SchoolERP.Domain.Student.Entities.StudentDocument;

namespace SchoolERP.Application.Features.Student.Mappings;

public class StudentMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ==================== Student ====================
        config.NewConfig<CreateStudentRequest, StudentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.EnrollmentId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Family)
            .Ignore(dest => dest.Enrollments)
            .Ignore(dest => dest.Parents)
            .Ignore(dest => dest.Documents);

        config.NewConfig<UpdateStudentRequest, StudentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.EnrollmentId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Family)
            .Ignore(dest => dest.Enrollments)
            .Ignore(dest => dest.Parents)
            .Ignore(dest => dest.Documents);

        config.NewConfig<PatchStudentRequest, StudentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.EnrollmentId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Family)
            .Ignore(dest => dest.Enrollments)
            .Ignore(dest => dest.Parents)
            .Ignore(dest => dest.Documents);

        config.NewConfig<StudentEntity, StudentResponse>()
            .Map(dest => dest.FamilyName,
                src => src.Family != null ? src.Family.HeadName : null);

        config.NewConfig<StudentEntity, StudentLightResponse>()
            .Map(dest => dest.FullName,
                src => (src.FirstName + " " + (src.LastName ?? "")).Trim());

        // ==================== Parent ====================
        config.NewConfig<CreateParentRequest, ParentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Student)
            .Ignore(dest => dest.Staff);

        config.NewConfig<UpdateParentRequest, ParentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Student)
            .Ignore(dest => dest.Staff);

        config.NewConfig<PatchParentRequest, ParentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.StudentId)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Student)
            .Ignore(dest => dest.Staff);

        config.NewConfig<ParentEntity, ParentResponse>()
            .Map(dest => dest.StaffName,
                src => src.Staff != null
                    ? (src.Staff.FirstName + " " + (src.Staff.LastName ?? "")).Trim()
                    : null);

        config.NewConfig<ParentEntity, ParentLightResponse>()
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

        config.NewConfig<StudentEnrollmentEntity, StudentEnrollmentResponse>();
        config.NewConfig<StudentEnrollmentEntity, StudentEnrollmentLightResponse>();

        // ==================== StudentDocument ====================
        config.NewConfig<CreateStudentDocumentRequest, StudentDocumentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Verified)
            .Ignore(dest => dest.UploadedBy)
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
            .Ignore(dest => dest.StudentId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Student);

        config.NewConfig<StudentDocumentEntity, StudentDocumentResponse>();
        config.NewConfig<StudentDocumentEntity, StudentDocumentLightResponse>();
    }
}
