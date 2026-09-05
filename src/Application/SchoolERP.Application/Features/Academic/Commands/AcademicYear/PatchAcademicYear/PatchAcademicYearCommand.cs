using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record PatchAcademicYearCommand(PatchAcademicYearRequest Request) : ICommand<AcademicYearResponse>;