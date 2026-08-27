using Mapster;
using SchoolERP.Application.Features.Branches.DTOs;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Mappings;

public class BranchMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // 🔥 CREATE: DTO → Entity (Null-safe)
        config.NewConfig<CreateBranchRequest, Branch>()
            .Map(dest => dest.Code, src => src.Code.Trim().ToUpper())
            .Map(dest => dest.Name, src => src.Name.Trim())
            // 🔥 NULL-SAFE: Agar Address null hai toh empty string assign karo
            .Map(dest => dest.Address, src => src.Address ?? string.Empty)
            .Map(dest => dest.Phone, src => src.Phone ?? string.Empty)
            .Map(dest => dest.Email, src => src.Email ?? string.Empty)
            .Map(dest => dest.ContactPerson, src => src.ContactPerson ?? string.Empty)
            .Map(dest => dest.IsDefault, src => src.IsDefault)
            .Map(dest => dest.Status, src => "active")
            .Map(dest => dest.Settings, src => "{}")
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Tenant);

        // 🔥 UPDATE: DTO → Existing Entity (Null-safe)
        config.NewConfig<UpdateBranchRequest, Branch>()
            .Map(dest => dest.Code, src => src.Code.Trim().ToUpper())
            .Map(dest => dest.Name, src => src.Name.Trim())
            .Map(dest => dest.Address, src => src.Address ?? string.Empty)
            .Map(dest => dest.Phone, src => src.Phone ?? string.Empty)
            .Map(dest => dest.Email, src => src.Email ?? string.Empty)
            .Map(dest => dest.ContactPerson, src => src.ContactPerson ?? string.Empty)
            .Map(dest => dest.IsDefault, src => src.IsDefault)
            .Map(dest => dest.Status, src => src.Status)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Tenant);

        // 🔥 PATCH: DTO → Existing Entity (Sirf non-null values)
        config.NewConfig<PatchBranchRequest, Branch>()
            .Map(dest => dest.Code, src => src.Code != null ? src.Code.Trim().ToUpper() : null)
            .Map(dest => dest.Name, src => src.Name != null ? src.Name.Trim() : null)
            .Map(dest => dest.Address, src => src.Address)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.ContactPerson, src => src.ContactPerson)
            .Map(dest => dest.IsDefault, src => src.IsDefault)
            .Map(dest => dest.Status, src => src.Status)
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Tenant);

        // 🔥 RESPONSE: Entity → DTO
        config.NewConfig<Branch, BranchResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.TenantId, src => src.TenantId)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Code, src => src.Code)
            .Map(dest => dest.Address, src => src.Address)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.ContactPerson, src => src.ContactPerson)
            .Map(dest => dest.IsDefault, src => src.IsDefault)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        // 🔥 LIGHT: Entity → LightResponse
        config.NewConfig<Branch, BranchLightResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Code, src => src.Code);
    }
}