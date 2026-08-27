using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Commands.Permission.CreatePermission;

public record CreatePermissionCommand(CreatePermissionRequest Request) : ICommand<PermissionResponse>;