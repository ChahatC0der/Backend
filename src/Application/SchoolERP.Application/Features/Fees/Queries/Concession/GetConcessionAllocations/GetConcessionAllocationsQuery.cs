using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Fees.DTOs.Concession;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Concession.GetConcessionAllocations;

public record class GetConcessionAllocationsQuery : PagedRequest, IQuery<PagedResponse<ConcessionAllocationResponse>>
{
    public long? StudentId { get; set; }
    public long? ConcessionId { get; set; }
    public long? AcademicYearId { get; set; }
    public string? Status { get; set; }   // pending | approved | rejected
}