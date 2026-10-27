using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Stop;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Route.GetStopsByRoute;

public sealed record GetStopsByRouteQuery(long RouteId) : IQuery<IReadOnlyList<StopResponse>>;
