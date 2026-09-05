using SchoolERP.Application.Common.Abstractions;

public record RestoreClassCommand(long Id) : ICommand<bool>;