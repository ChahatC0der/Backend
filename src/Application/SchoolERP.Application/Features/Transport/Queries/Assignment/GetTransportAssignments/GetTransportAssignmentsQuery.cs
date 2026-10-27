using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Transport.DTOs.Assignment;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Assignment.GetTransportAssignments;

public sealed record GetTransportAssignmentsQuery : PagedRequest, IQuery<PagedResponse<TransportAssignmentResponse>>
{
    public long? AcademicYearId { get; set; }
    public long? PickupRouteId { get; set; }
    public long? PickupStopId { get; set; }
    public long? PickupVehicleId { get; set; }
    public bool? IsCurrent { get; set; }
}
