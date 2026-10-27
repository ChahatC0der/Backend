using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.RouteVehicle;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Assignment.GetRouteVehicleHistory;

/// <summary>Either RouteId or VehicleId (or both) can be filtered. If neither provided, returns recent 500.</summary>
public sealed record GetRouteVehicleHistoryQuery(long? RouteId = null, long? VehicleId = null) : IQuery<IReadOnlyList<RouteVehicleAssignmentResponse>>;
