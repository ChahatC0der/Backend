using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Queries.Role.GetRoles;

public record GetRolesQuery(GetRolesRequest Request) : IQuery<PagedResponse<RoleResponse>>;