using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Master.DTOs;

public record BulkPatchMasterItemCommand(BulkPatchMasterItemRequest Request) : ICommand<bool>;