using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Tenants.DTOs;

namespace SchoolERP.Application.Features.Tenants.Commands.BulkUpdate;

public record BulkUpdateTenantCommand(BulkUpdateTenantRequest Request) : ICommand<int>;