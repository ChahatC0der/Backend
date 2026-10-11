using SchoolERP.Application.Common.Abstractions;

public record RestoreModuleCommand(long Id) : ICommand<bool>;