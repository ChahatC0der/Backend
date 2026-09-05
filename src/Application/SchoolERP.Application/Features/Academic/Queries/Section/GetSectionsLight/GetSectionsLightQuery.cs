using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record GetSectionsLightQuery : IQuery<List<SectionLightResponse>>;