using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Branches.DTOs;

namespace SchoolERP.Application.Features.Branches.Commands.BulkPatch;

public record BulkPatchBranchCommand(BulkPatchBranchRequest Request) : ICommand<int>;