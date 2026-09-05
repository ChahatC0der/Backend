using SchoolERP.Application.Common.Abstractions;

public record DeleteClassCommand(long Id) : ICommand<bool>;