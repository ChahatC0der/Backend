using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Concession.ExportConcessions;

public sealed record ExportConcessionsQuery : IQuery<byte[]>
{
    public string? Category { get; init; }
    public bool? IsAutoApply { get; init; }
    public string? SearchTerm { get; init; }
}