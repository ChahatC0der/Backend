using Mapster;
using SchoolERP.Application.Features.Master.DTOs;
using SchoolERP.Domain.Master.Entities;

namespace SchoolERP.Application.Features.Master.Mappings;

public class MasterMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ==================== MasterCategory ====================
        config.NewConfig<CreateMasterCategoryRequest, MasterCategory>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Module)
            .Ignore(dest => dest.Items);

        config.NewConfig<UpdateMasterCategoryRequest, MasterCategory>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Module)
            .Ignore(dest => dest.Items);

        config.NewConfig<PatchMasterCategoryRequest, MasterCategory>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Module)
            .Ignore(dest => dest.Items);

        config.NewConfig<MasterCategory, MasterCategoryResponse>();
        config.NewConfig<MasterCategory, MasterCategoryLightResponse>();

        // ==================== MasterItem ====================
        config.NewConfig<CreateMasterItemRequest, MasterItem>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Category);

        config.NewConfig<UpdateMasterItemRequest, MasterItem>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Category);

        config.NewConfig<PatchMasterItemRequest, MasterItem>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Category);

        config.NewConfig<MasterItem, MasterItemResponse>();
        config.NewConfig<MasterItem, MasterItemLightResponse>();
    }
}