using SchoolERP.Application.Common.Abstractions;

public record DeleteModuleCommand(long Id) : ICommand<bool>;