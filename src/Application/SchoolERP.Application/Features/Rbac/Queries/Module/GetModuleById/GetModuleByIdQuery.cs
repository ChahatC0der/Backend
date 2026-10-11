using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Rbac.DTOs;

public record GetModuleByIdQuery(long Id) : IQuery<ModuleResponse>;