using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Master.DTOs;

public record PatchMasterItemCommand(PatchMasterItemRequest Request) : ICommand<MasterItemResponse>;