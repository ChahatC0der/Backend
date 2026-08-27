using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Queries.Role.GetRoleById;

public record GetRoleByIdQuery(long RoleId) : IQuery<RoleResponse>;