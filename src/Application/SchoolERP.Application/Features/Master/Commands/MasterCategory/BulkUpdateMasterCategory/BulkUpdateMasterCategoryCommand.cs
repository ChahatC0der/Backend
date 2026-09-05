using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Master.DTOs;

public record BulkUpdateMasterCategoryCommand(BulkUpdateMasterCategoryRequest Request) : ICommand<bool>;