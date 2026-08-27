using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Queries.Role.GetAllRolesLight;

public record GetAllRolesLightQuery : IQuery<List<RoleLightResponse>>;