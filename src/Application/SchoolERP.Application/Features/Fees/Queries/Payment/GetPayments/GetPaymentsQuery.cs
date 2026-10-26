using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Fees.DTOs.Payment;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Payment.GetPayments;

public sealed record GetPaymentsQuery : PagedRequest, IQuery<PagedResponse<PaymentResponse>>
{
    public long? StudentId { get; set; }
    public PaymentStatus? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
