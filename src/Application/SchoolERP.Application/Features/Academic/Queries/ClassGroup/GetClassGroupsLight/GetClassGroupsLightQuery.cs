using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record GetClassGroupsLightQuery : IQuery<List<ClassGroupLightResponse>>;