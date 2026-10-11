using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Rbac.DTOs;

public record UpdateModuleCommand(UpdateModuleRequest Request) : ICommand<ModuleResponse>;