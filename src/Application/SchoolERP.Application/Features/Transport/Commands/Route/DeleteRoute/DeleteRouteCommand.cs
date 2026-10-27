using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.Route.DeleteRoute;

public record DeleteRouteCommand(long Id) : ICommand<bool>;
