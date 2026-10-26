using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.Invoice.GetStudentFeeCounter;

/// <summary>
/// Core counter screen: student + structure + wallet + pending invoices (with lines).
/// Pure EF Core — 6 indexed queries in a single roundtrip batch. Fast enough for Phase 1.
/// </summary>
public sealed record GetStudentFeeCounterQuery(long StudentId, long? AcademicYearId = null)
    : IQuery<StudentFeeCounterResponse>;
