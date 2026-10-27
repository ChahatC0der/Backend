using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Transport.DTOs.Route;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Transport.Enums;

namespace SchoolERP.Application.Features.Transport.Queries.Route.GetRoutes;

public sealed record GetRoutesQuery : PagedRequest, IQuery<PagedResponse<RouteResponse>>
{
    public RouteStatus? Status { get; set; }
}
