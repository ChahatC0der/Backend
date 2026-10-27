using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Stop;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.Stop.ReorderStops;

public record ReorderStopsCommand(long RouteId, ReorderStopsRequest Request) : ICommand<IReadOnlyList<StopResponse>>;
