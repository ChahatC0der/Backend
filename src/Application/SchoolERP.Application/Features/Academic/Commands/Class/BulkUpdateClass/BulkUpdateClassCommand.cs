using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record BulkUpdateClassCommand(BulkUpdateClassRequest Request) : ICommand<bool>;