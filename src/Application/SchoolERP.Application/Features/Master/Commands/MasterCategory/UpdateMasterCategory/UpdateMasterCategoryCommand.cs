using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Master.DTOs;

public record UpdateMasterCategoryCommand(UpdateMasterCategoryRequest Request) : ICommand<MasterCategoryResponse>;