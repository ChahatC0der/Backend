using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.Vehicle.DeleteVehicle;

public record DeleteVehicleCommand(long Id) : ICommand<bool>;
