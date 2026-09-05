using SchoolERP.Application.Common.Abstractions;

public record RestoreSectionCommand(long Id) : ICommand<bool>;