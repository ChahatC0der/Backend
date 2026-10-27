using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.DriverAssignment;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Assignment.GetVehicleDriverHistory;

public sealed record GetVehicleDriverHistoryQuery(long? VehicleId = null, long? DriverStaffId = null) : IQuery<IReadOnlyList<DriverAssignmentResponse>>;
