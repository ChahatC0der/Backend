using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Route;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Route.GetRoutesLight;

public sealed record GetRoutesLightQuery : IQuery<IReadOnlyList<RouteLightResponse>>;
