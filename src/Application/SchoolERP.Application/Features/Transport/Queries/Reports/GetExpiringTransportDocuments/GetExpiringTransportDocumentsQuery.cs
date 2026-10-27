using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Reports.GetExpiringTransportDocuments;

/// <summary>Phase 1: vehicles only. Driver license expiry deferred (Staff module).</summary>
public sealed record GetExpiringTransportDocumentsQuery(int DaysAhead = 30) : IQuery<IReadOnlyList<ExpiringDocumentAlertRow>>;
