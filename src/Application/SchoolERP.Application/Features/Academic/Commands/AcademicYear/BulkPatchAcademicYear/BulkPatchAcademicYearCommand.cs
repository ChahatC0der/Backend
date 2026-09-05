using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record BulkPatchAcademicYearCommand(BulkPatchAcademicYearRequest Request) : ICommand<bool>;