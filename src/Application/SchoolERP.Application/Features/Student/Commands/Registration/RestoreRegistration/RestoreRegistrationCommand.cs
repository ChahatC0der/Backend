using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.Registration.RestoreRegistration;

public record RestoreRegistrationCommand(long Id) : ICommand<bool>;
