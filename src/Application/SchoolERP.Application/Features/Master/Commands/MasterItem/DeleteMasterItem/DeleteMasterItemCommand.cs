using SchoolERP.Application.Common.Abstractions;

public record DeleteMasterItemCommand(long Id) : ICommand<bool>;