using Mapster;
using SchoolERP.Application.Features.Staff.DTOs;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;
using DepartmentEntity = SchoolERP.Domain.Staff.Entities.Department;
using StaffDocumentEntity = SchoolERP.Domain.Staff.Entities.StaffDocument;

namespace SchoolERP.Application.Features.Staff.Mappings;

public class StaffMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ==================== Department ====================
        config.NewConfig<CreateDepartmentRequest, DepartmentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Hod)
            .Ignore(dest => dest.StaffMembers);

        config.NewConfig<UpdateDepartmentRequest, DepartmentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Hod)
            .Ignore(dest => dest.StaffMembers);

        config.NewConfig<PatchDepartmentRequest, DepartmentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Hod)
            .Ignore(dest => dest.StaffMembers);

        config.NewConfig<DepartmentEntity, DepartmentResponse>()
            .Map(dest => dest.HodName,
                 src => src.Hod != null ? (src.Hod.FirstName + " " + (src.Hod.LastName ?? "")).Trim() : null)
            .Map(dest => dest.StaffCount, src => src.StaffMembers.Count);

        config.NewConfig<DepartmentEntity, DepartmentLightResponse>();

        // ==================== Staff ====================
        config.NewConfig<CreateStaffRequest, StaffEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.StaffId)              // auto-generated
            .Ignore(dest => dest.Status)               // default "active"
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Department)
            .Ignore(dest => dest.Documents);

        config.NewConfig<UpdateStaffRequest, StaffEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.StaffId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Department)
            .Ignore(dest => dest.Documents);

        config.NewConfig<PatchStaffRequest, StaffEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.StaffId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Department)
            .Ignore(dest => dest.Documents);

        config.NewConfig<StaffEntity, StaffResponse>()
            .Map(dest => dest.DepartmentName, src => src.Department != null ? src.Department.Name : null);

        config.NewConfig<StaffEntity, StaffLightResponse>()
            .Map(dest => dest.FullName,
                 src => (src.FirstName + " " + (src.LastName ?? "")).Trim());

        // ==================== StaffDocument ====================
        config.NewConfig<CreateStaffDocumentRequest, StaffDocumentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.VerificationStatus)   // default "not_verified"
            .Ignore(dest => dest.VerifiedBy)
            .Ignore(dest => dest.VerifiedAt)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Staff);

        config.NewConfig<UpdateStaffDocumentRequest, StaffDocumentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Staff);

        config.NewConfig<PatchStaffDocumentRequest, StaffDocumentEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Staff);

        config.NewConfig<StaffDocumentEntity, StaffDocumentResponse>();
        config.NewConfig<StaffDocumentEntity, StaffDocumentLightResponse>();
    }
}