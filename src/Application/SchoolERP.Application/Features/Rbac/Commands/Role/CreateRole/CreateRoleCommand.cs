using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Commands.Role.CreateRole;

public record CreateRoleCommand(CreateRoleRequest Request) : ICommand<RoleResponse>;