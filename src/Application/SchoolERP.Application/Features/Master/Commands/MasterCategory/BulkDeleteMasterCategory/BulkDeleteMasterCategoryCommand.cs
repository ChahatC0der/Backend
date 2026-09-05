using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Master.DTOs;

public record BulkDeleteMasterCategoryCommand(BulkDeleteMasterCategoryRequest Request) : ICommand<bool>;