using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Tenants.DTOs;

namespace SchoolERP.Application.Features.Tenants.Commands.UpdateTenant;

public record UpdateTenantCommand(Guid Id, UpdateTenantRequest Request) : ICommand<TenantResponse>;