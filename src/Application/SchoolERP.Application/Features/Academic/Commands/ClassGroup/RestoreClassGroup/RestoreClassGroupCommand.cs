using SchoolERP.Application.Common.Abstractions;

public record RestoreClassGroupCommand(long Id) : ICommand<bool>;