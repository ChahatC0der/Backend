using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Academic.Commands.ClassGroup.CreateClassGroup;

public record CreateClassGroupCommand(CreateClassGroupRequest Request) : ICommand<ClassGroupResponse>;