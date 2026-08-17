using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Tenants.DTOs;

namespace SchoolERP.Application.Features.Tenants.Commands.BulkDelete;

public record BulkDeleteTenantCommand(BulkDeleteRequest Request) : ICommand<int>;