using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.TransportFee.DeleteTransportFee;

public record DeleteTransportFeeCommand(long Id) : ICommand<bool>;
