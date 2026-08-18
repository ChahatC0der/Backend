using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Branches.DTOs;

namespace SchoolERP.Application.Features.Branches.Commands.BulkDelete;

public record BulkDeleteBranchCommand(Guid TenantId, BulkDeleteBranchRequest Request) : ICommand<int>;