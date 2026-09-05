using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record BulkPatchClassGroupCommand(BulkPatchClassGroupRequest Request) : ICommand<bool>;