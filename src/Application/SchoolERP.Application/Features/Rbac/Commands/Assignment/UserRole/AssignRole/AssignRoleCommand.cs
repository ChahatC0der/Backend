using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Application.Common.Abstractions;

public record AssignRoleCommand(AssignRoleRequest Request) : ICommand<RoleAssignmentResponse>;