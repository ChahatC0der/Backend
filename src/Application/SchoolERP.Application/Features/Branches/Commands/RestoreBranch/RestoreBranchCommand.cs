using MediatR;
using SchoolERP.Application.Common.Abstractions;

namespace SchoolERP.Application.Features.Branches.Commands.RestoreBranch;

public record RestoreBranchCommand(Guid TenantId, Guid BranchId) : ICommand<bool>;