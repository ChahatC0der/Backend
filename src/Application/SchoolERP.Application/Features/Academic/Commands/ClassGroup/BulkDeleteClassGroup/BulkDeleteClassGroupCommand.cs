using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record BulkDeleteClassGroupCommand(BulkDeleteClassGroupRequest Request) : ICommand<bool>;