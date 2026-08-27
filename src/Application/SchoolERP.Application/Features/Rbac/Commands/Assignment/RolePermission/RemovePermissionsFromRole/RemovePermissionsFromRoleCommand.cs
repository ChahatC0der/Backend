using SchoolERP.Application.Common.Abstractions;

public record RemovePermissionsFromRoleCommand(long RoleId, List<long> PermissionIds) : ICommand<bool>;