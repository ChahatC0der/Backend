using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;

public record UpdateClassGroupCommand(UpdateClassGroupRequest Request) : ICommand<ClassGroupResponse>;