using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Payment;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Payment.GetStudentCreditHistory;

public sealed record GetStudentCreditHistoryQuery(long StudentId) : IQuery<CreditHistoryResponse>;
