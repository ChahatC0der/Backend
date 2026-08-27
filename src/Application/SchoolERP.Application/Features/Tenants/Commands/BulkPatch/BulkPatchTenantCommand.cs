using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Tenants.DTOs;

namespace SchoolERP.Application.Features.Tenants.Commands.BulkPatch;

public record BulkPatchTenantCommand(BulkPatchTenantRequest Request) : ICommand<int>;