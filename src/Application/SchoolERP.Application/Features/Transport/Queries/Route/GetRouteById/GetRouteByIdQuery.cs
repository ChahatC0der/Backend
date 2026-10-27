using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Route;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Route.GetRouteById;

public sealed record GetRouteByIdQuery(long Id) : IQuery<RouteResponse>;
