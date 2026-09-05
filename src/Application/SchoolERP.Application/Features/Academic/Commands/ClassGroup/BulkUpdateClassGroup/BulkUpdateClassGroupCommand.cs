using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record BulkUpdateClassGroupCommand(BulkUpdateClassGroupRequest Request) : ICommand<bool>;