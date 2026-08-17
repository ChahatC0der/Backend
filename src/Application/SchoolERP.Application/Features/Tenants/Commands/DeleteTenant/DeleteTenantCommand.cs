using MediatR;
using SchoolERP.Application.Common.Abstractions;

namespace SchoolERP.Application.Features.Tenants.Commands.DeleteTenant;

public record DeleteTenantCommand(Guid Id) : ICommand<bool>;