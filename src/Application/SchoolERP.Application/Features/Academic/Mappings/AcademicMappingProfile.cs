using Mapster;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Academic.Entities;

namespace SchoolERP.Application.Features.Academic.Mappings;

public class AcademicMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ==================== AcademicYear ====================
        config.NewConfig<CreateAcademicYearRequest, AcademicYear>()
            .Ignore(dest => dest.Id)
            //.Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt);

        config.NewConfig<UpdateAcademicYearRequest, AcademicYear>()
            .Ignore(dest => dest.Id)
            //.Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt);

        config.NewConfig<AcademicYear, AcademicYearResponse>();
        config.NewConfig<AcademicYear, AcademicYearLightResponse>();

        // ==================== ClassGroup ====================
        config.NewConfig<CreateClassGroupRequest, ClassGroup>()
            .Ignore(dest => dest.Id)
            //.Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt);

        config.NewConfig<UpdateClassGroupRequest, ClassGroup>()
            .Ignore(dest => dest.Id)
            //.Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt);

        config.NewConfig<ClassGroup, ClassGroupResponse>();
        config.NewConfig<ClassGroup, ClassGroupLightResponse>();

        // ==================== Class ====================
        config.NewConfig<CreateClassRequest, Class>()
            .Ignore(dest => dest.Id)
            //.Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.ClassGroup)
            .Ignore(dest => dest.Sections);

        config.NewConfig<UpdateClassRequest, Class>()
            .Ignore(dest => dest.Id)
            //.Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.ClassGroup)
            .Ignore(dest => dest.Sections);

        config.NewConfig<Class, ClassResponse>();
        config.NewConfig<Class, ClassLightResponse>();

        // ==================== Section ====================
        config.NewConfig<CreateSectionRequest, Section>()
            .Ignore(dest => dest.Id)
            //.Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Class);

        config.NewConfig<UpdateSectionRequest, Section>()
            .Ignore(dest => dest.Id)
            //.Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Class);

        config.NewConfig<Section, SectionResponse>();
        config.NewConfig<Section, SectionLightResponse>();
    }
}