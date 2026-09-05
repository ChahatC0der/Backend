using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record UpdateAcademicYearCommand(UpdateAcademicYearRequest Request) : ICommand<AcademicYearResponse>;