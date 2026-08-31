using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Branches.DTOs;

namespace SchoolERP.Application.Features.Branches.Queries.GetBranchesLight;

public record GetBranchesLightQuery() : IQuery<IEnumerable<BranchLightResponse>>;