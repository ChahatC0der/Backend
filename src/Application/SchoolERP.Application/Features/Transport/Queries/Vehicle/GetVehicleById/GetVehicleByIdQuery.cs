using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Vehicle;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Vehicle.GetVehicleById;

public sealed record GetVehicleByIdQuery(long Id) : IQuery<VehicleResponse>;
