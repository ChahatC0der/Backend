using MediatR;
using SchoolERP.Application.Common.Abstractions;

namespace SchoolERP.Application.Features.Tenants.Queries.ExportTenants;

public record ExportTenantsQuery() : IQuery<byte[]>;