using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Commands.User.DeleteUser;

public record DeleteUserCommand(long UserId) : ICommand<bool>;