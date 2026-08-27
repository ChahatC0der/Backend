using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Application.Common.Abstractions;

public record UpdatePermissionCommand(UpdatePermissionRequest Request) : ICommand<PermissionResponse>;