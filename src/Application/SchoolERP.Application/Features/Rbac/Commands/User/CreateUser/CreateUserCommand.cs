using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Rbac.DTOs;

public record CreateUserCommand(CreateUserRequest Request) : ICommand<UserResponse>;