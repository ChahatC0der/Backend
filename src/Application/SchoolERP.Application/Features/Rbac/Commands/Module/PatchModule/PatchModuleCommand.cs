using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Rbac.DTOs;

public record PatchModuleCommand(PatchModuleRequest Request) : ICommand<ModuleResponse>;