using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Tenants.DTOs;

namespace SchoolERP.Application.Features.Tenants.Commands.CreateTenant;

public record CreateTenantCommand(CreateTenantRequest Request) : ICommand<TenantResponse>;