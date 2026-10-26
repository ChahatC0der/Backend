using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Invoice.GetInvoices;

public sealed record GetInvoicesQuery : PagedRequest, IQuery<PagedResponse<InvoiceListResponse>>
{
    public long? AcademicYearId { get; set; }
    public long? StudentId { get; set; }
    public long? ClassId { get; set; }
    public long? SectionId { get; set; }
    public InvoiceStatus? Status { get; set; }
    public string? PeriodLabel { get; set; }
    public DateTime? DueDateFrom { get; set; }
    public DateTime? DueDateTo { get; set; }
}
