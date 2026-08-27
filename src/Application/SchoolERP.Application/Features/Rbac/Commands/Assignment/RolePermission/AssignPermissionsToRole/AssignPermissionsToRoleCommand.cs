using SchoolERP.Application.Common.Abstractions;

public record AssignPermissionsToRoleCommand(long RoleId, List<long> PermissionIds) : ICommand<bool>;