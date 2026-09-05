using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Master.DTOs;

public record BulkPatchMasterCategoryCommand(BulkPatchMasterCategoryRequest Request) : ICommand<bool>;