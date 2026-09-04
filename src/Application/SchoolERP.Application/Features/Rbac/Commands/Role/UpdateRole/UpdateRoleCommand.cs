using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Application.Common.Abstractions;

public record UpdateRoleCommand(long id,UpdateRoleRequest Request) : ICommand<RoleResponse>;