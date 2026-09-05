using SchoolERP.Application.Common.Abstractions;

public record RestoreMasterCategoryCommand(long Id) : ICommand<bool>;