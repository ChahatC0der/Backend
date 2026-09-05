using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record PatchClassGroupCommand(PatchClassGroupRequest Request) : ICommand<ClassGroupResponse>;