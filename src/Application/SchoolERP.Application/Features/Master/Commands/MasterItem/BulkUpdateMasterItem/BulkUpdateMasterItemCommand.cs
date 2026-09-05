using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Master.DTOs;

public record BulkUpdateMasterItemCommand(BulkUpdateMasterItemRequest Request) : ICommand<bool>;