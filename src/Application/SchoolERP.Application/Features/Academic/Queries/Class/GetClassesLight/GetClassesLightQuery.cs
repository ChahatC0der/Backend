using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record GetClassesLightQuery : IQuery<List<ClassLightResponse>>;