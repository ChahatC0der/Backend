using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Reports;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Reports.GetStudentLedger;

public sealed record GetStudentLedgerQuery(long StudentId, long? AcademicYearId = null) : IQuery<IReadOnlyList<StudentLedgerRow>>;
