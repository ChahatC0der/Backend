using Mapster;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Rbac.Entities;

namespace SchoolERP.Application.Features.Rbac.Mappings;

public class RbacMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ===== Module =====
        config.NewConfig<CreateModuleRequest, Module>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.IsSystem)   // only platform admin can set, not via API
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Parent)
            .Ignore(dest => dest.Children)
            .Ignore(dest => dest.Permissions)
            .Map(dest => dest.Key, src => src.Key.Trim().ToLowerInvariant());

        config.NewConfig<UpdateModuleRequest, Module>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.IsSystem)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Parent)
            .Ignore(dest => dest.Children)
            .Ignore(dest => dest.Permissions)
            .Map(dest => dest.Key, src => src.Key.Trim().ToLowerInvariant());

        config.NewConfig<PatchModuleRequest, Module>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.IsSystem)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.Parent)
            .Ignore(dest => dest.Children)
            .Ignore(dest => dest.Permissions);

        config.NewConfig<Module, ModuleResponse>();
        config.NewConfig<Module, ModuleLightResponse>();

        // ===== Role =====
        config.NewConfig<CreateRoleRequest, Role>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            //.Ignore(dest => dest.CreatedBy)
            //.Ignore(dest => dest.UpdatedBy)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.RolePermissions)
            .Ignore(dest => dest.UserRoles)
            .Ignore(dest => dest.BaseRole)
            .Map(dest => dest.Code, src => src.Code.Trim().ToUpperInvariant());

        config.NewConfig<UpdateRoleRequest, Role>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.CreatedAt)
            .Ignore(dest => dest.UpdatedAt)
            //.Ignore(dest => dest.CreatedBy)
            //.Ignore(dest => dest.UpdatedBy)
            .Ignore(dest => dest.IsDeleted)
            .Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.RolePermissions)
            .Ignore(dest => dest.UserRoles)
            .Ignore(dest => dest.BaseRole)
            .Map(dest => dest.Code, src => src.Code.Trim().ToUpperInvariant());

        // Role -> RoleResponse (manual mapping for Permissions)
        config.NewConfig<Role, RoleResponse>()
            .Map(dest => dest.Permissions, src => src.RolePermissions.Select(rp => rp.Permission.Adapt<PermissionResponse>()).ToList());

        // Permission -> PermissionResponse
        config.NewConfig<Permission, PermissionResponse>()
            .Map(dest => dest.ModuleKey, src => src.Module.Key);

        // AssignRoleRequest -> UserRole
        config.NewConfig<AssignRoleRequest, UserRole>()
            //.Ignore(dest => dest.Id)
            //.Ignore(dest => dest.TenantId)
            //.Ignore(dest => dest.CreatedAt)
            //.Ignore(dest => dest.CreatedBy)
            //.Ignore(dest => dest.UpdatedAt)
            //.Ignore(dest => dest.UpdatedBy)
            //.Ignore(dest => dest.IsDeleted)
            //.Ignore(dest => dest.DeletedAt)
            .Ignore(dest => dest.User)
            .Ignore(dest => dest.Role);

        // UserRole -> RoleAssignmentResponse
        config.NewConfig<UserRole, RoleAssignmentResponse>()
            //.Map(dest => dest.UserRoleId, src => src.Id)
            .Map(dest => dest.RoleName, src => src.Role.Name)
            .Map(dest => dest.RoleCode, src => src.Role.Code);

        // BulkRoleJob -> BulkRoleJobResponse
        config.NewConfig<BulkRoleJob, BulkRoleJobResponse>();
    }
}