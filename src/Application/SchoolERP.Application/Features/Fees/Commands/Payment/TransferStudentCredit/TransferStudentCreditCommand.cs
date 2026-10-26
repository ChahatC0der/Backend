using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Payment;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Payment.TransferStudentCredit;

public record TransferStudentCreditCommand(TransferCreditRequest Request) : ICommand<TransferCreditResponse>;

public sealed record TransferCreditResponse(long FromStudentId, long ToStudentId, decimal Amount, decimal FromBalanceAfter, decimal ToBalanceAfter);
