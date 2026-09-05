using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Master.DTOs;

public record PatchMasterCategoryCommand(PatchMasterCategoryRequest Request) : ICommand<MasterCategoryResponse>;