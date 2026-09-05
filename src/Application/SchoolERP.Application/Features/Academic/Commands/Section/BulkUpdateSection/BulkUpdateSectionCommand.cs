using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record BulkUpdateSectionCommand(BulkUpdateSectionRequest Request) : ICommand<bool>;