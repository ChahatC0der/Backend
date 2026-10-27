using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Route;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.Route.PatchRouteStatus;

public record PatchRouteStatusCommand(long Id, PatchRouteStatusRequest Request) : ICommand<RouteResponse>;
