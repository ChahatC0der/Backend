using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Application.Common.Abstractions;

public record UpdateRoleCommand(UpdateRoleRequest Request) : ICommand<RoleResponse>;