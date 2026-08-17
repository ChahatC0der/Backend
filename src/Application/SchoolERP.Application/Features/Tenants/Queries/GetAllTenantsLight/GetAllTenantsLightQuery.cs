using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Tenants.DTOs;

namespace SchoolERP.Application.Features.Tenants.Queries.GetAllTenantsLight;

public record GetAllTenantsLightQuery() : IQuery<IEnumerable<TenantLightResponse>>;