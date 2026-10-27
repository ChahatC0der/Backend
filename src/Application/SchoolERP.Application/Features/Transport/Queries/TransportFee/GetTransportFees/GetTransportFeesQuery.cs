using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.TransportFee;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.TransportFee.GetTransportFees;

public sealed class GetTransportFeesQuery : IQuery<IReadOnlyList<TransportFeeResponse>>
{
    public long? AcademicYearId { get; set; }
    public long? RouteId { get; set; }
    public long? StopId { get; set; }
}
