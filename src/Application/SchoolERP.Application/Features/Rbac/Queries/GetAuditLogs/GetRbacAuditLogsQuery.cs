using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Rbac.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Rbac.Queries.AuditLog.GetRbacAuditLogs;

public record GetRbacAuditLogsQuery(GetRbacAuditLogsRequest Request) : IQuery<PagedResponse<RbacAuditLogResponse>>;