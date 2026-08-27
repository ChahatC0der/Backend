using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Queries.User.GetUserRoles;

public record GetUserRolesQuery(long UserId) : IQuery<List<RoleAssignmentResponse>>;