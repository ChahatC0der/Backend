using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Auth.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Auth.Commands.Login;

public record LoginCommand(LoginRequest Request) : ICommand<LoginResponse>;