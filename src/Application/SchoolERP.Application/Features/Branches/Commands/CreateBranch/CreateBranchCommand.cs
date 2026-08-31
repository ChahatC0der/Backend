using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Branches.DTOs;

namespace SchoolERP.Application.Features.Branches.Commands.CreateBranch;

public record CreateBranchCommand(CreateBranchRequest Request) : ICommand<BranchResponse>;