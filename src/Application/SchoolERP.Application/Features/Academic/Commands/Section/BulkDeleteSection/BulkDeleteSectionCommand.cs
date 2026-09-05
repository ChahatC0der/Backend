using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record BulkDeleteSectionCommand(BulkDeleteSectionRequest Request) : ICommand<bool>;