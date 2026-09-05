using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record BulkPatchClassCommand(BulkPatchClassRequest Request) : ICommand<bool>;