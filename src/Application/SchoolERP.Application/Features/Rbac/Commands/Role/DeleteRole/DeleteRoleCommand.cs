using SchoolERP.Application.Common.Abstractions;

public record DeleteRoleCommand(long RoleId) : ICommand<bool>;