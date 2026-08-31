using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Branches.DTOs;

namespace SchoolERP.Application.Features.Branches.Commands.PatchBranch;

public record PatchBranchCommand( Guid BranchId, PatchBranchRequest Request) : ICommand<BranchResponse>;