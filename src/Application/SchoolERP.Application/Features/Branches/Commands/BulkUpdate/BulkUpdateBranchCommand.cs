using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Branches.DTOs;

namespace SchoolERP.Application.Features.Branches.Commands.BulkUpdate;

public record BulkUpdateBranchCommand(Guid TenantId, BulkUpdateBranchRequest Request) : ICommand<int>;