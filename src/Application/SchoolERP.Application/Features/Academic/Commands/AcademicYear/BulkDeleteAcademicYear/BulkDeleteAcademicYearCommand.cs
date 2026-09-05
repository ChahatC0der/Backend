using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record BulkDeleteAcademicYearCommand(BulkDeleteAcademicYearRequest Request) : ICommand<bool>;