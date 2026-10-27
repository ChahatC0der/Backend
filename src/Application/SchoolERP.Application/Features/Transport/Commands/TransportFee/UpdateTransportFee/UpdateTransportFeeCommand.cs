using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.TransportFee;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.TransportFee.UpdateTransportFee;

public record UpdateTransportFeeCommand(long Id, UpdateTransportFeeRequest Request) : ICommand<TransportFeeResponse>;
