using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record GetAcademicYearByIdQuery(long Id) : IQuery<AcademicYearResponse>;