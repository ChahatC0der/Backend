using MediatR;
using SchoolERP.Application.Common.Abstractions;

namespace SchoolERP.Application.Features.Tenants.Commands.RestoreTenant;

public record RestoreTenantCommand(Guid Id) : ICommand<bool>;