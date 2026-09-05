using SchoolERP.Application.Common.Abstractions;

public record RestoreMasterItemCommand(long Id) : ICommand<bool>;