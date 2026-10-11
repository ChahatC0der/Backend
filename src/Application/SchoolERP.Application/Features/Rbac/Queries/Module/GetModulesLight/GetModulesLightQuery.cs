using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Rbac.DTOs;

public record GetModulesLightQuery : IQuery<List<ModuleLightResponse>>;