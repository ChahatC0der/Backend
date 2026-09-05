using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record BulkUpdateAcademicYearCommand(BulkUpdateAcademicYearRequest Request) : ICommand<bool>;