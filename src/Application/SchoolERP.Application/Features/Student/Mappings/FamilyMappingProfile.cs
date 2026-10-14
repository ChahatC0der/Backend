using Mapster;
using SchoolERP.Application.Features.Student.DTOs;
using FamilyEntity = SchoolERP.Domain.Student.Entities.Family;

namespace SchoolERP.Application.Features.Student.Mappings;

public class FamilyMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateFamilyRequest, FamilyEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Students);

        config.NewConfig<UpdateFamilyRequest, FamilyEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Students);

        config.NewConfig<PatchFamilyRequest, FamilyEntity>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.BranchId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Students);

        config.NewConfig<FamilyEntity, FamilyResponse>()
            .Map(dest => dest.StudentCount, src => src.Students.Count);

        config.NewConfig<FamilyEntity, FamilyLightResponse>()
            .Map(dest => dest.StudentCount, src => src.Students.Count);
    }
}
