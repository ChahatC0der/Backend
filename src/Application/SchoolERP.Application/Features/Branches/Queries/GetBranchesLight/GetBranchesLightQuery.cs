using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Branches.DTOs;

namespace SchoolERP.Application.Features.Branches.Queries.GetBranchesLight;

public record GetBranchesLightQuery(Guid TenantId) : IQuery<IEnumerable<BranchLightResponse>>;