using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record GetAcademicYearsLightQuery : IQuery<List<AcademicYearLightResponse>>;