using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Transport.DTOs.Vehicle;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Transport.Enums;

namespace SchoolERP.Application.Features.Transport.Queries.Vehicle.GetVehicles;

public sealed record GetVehiclesQuery : PagedRequest, IQuery<PagedResponse<VehicleResponse>>
{
    public VehicleStatus? Status { get; set; }
    public string? VehicleType { get; set; }
}
