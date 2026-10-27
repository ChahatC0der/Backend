using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.Vehicle.RestoreVehicle;

public record RestoreVehicleCommand(long Id) : ICommand<bool>;
