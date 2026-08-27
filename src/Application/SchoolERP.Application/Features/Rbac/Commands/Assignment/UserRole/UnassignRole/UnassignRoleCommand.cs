using SchoolERP.Application.Common.Abstractions;

public record UnassignRoleCommand(long UserRoleId) : ICommand<bool>;