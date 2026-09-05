using SchoolERP.Application.Common.Abstractions;

public record DeleteClassGroupCommand(long Id) : ICommand<bool>;