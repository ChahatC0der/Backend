using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record BulkDeleteClassCommand(BulkDeleteClassRequest Request) : ICommand<bool>;