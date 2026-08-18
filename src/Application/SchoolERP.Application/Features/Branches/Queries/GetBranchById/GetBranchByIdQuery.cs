using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Branches.DTOs;

namespace SchoolERP.Application.Features.Branches.Queries.GetBranchById;

public record GetBranchByIdQuery(Guid TenantId, Guid BranchId) : IQuery<BranchResponse>;