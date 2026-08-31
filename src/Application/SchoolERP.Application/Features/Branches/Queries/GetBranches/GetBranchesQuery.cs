using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Branches.DTOs;

namespace SchoolERP.Application.Features.Branches.Queries.GetBranches;

public record GetBranchesQuery(PagedRequest Request) : IQuery<PagedResponse<BranchResponse>>;