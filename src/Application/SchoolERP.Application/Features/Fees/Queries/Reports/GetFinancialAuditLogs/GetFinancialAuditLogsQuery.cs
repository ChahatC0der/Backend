using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Reports.GetFinancialAuditLogs;

public sealed class GetFinancialAuditLogsQuery : IQuery<IReadOnlyList<AuditLogRow>>
{
    public string? TableName { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Limit { get; set; } = 500;
}
