using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Tenants.DTOs;

namespace SchoolERP.Application.Features.Tenants.Queries.GetAllTenants;

public record GetAllTenantsQuery(PagedRequest Request) : IQuery<PagedResponse<TenantResponse>>;