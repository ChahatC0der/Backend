using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Stop;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.Stop.UpdateStop;

public record UpdateStopCommand(long RouteId, long StopId, UpdateStopRequest Request) : ICommand<StopResponse>;
