using MediatR;
using SchoolERP.Application.Common.Abstractions;

namespace SchoolERP.Application.Features.Branches.Commands.DeleteBranch;

public record DeleteBranchCommand(Guid BranchId) : ICommand<bool>;