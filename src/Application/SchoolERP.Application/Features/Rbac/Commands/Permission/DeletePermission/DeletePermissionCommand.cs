using SchoolERP.Application.Common.Abstractions;

public record DeletePermissionCommand(long PermissionId) : ICommand<bool>;