using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Branches.DTOs;

namespace SchoolERP.Application.Features.Branches.Commands.UpdateBranch;

public record UpdateBranchCommand(Guid TenantId, Guid BranchId, UpdateBranchRequest Request) : ICommand<BranchResponse>;