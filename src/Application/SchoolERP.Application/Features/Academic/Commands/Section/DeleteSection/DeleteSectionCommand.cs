using SchoolERP.Application.Common.Abstractions;

public record DeleteSectionCommand(long Id) : ICommand<bool>;