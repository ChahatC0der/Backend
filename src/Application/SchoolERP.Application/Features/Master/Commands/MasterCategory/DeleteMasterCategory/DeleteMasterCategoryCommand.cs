using SchoolERP.Application.Common.Abstractions;

public record DeleteMasterCategoryCommand(long Id) : ICommand<bool>;