using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.TransportFee;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.TransportFee.CreateTransportFee;

public record CreateTransportFeeCommand(CreateTransportFeeRequest Request) : ICommand<TransportFeeResponse>;
