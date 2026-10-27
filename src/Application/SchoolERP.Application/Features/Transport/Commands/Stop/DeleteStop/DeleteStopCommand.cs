using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.Stop.DeleteStop;

public record DeleteStopCommand(long RouteId, long StopId) : ICommand<bool>;
