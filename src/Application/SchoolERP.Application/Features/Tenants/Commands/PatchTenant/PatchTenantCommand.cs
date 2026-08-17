using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Tenants.DTOs;

namespace SchoolERP.Application.Features.Tenants.Commands.PatchTenant;

public record PatchTenantCommand(Guid Id, PatchTenantRequest Request) : ICommand<TenantResponse>;