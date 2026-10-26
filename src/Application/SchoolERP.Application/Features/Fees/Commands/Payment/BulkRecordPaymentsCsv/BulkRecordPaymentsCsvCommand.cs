using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.Payment;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.Payment.BulkRecordPaymentsCsv;

public record BulkRecordPaymentsCsvCommand(long AcademicYearId, IReadOnlyList<BulkCsvRowRequest> Rows) : ICommand<BulkCsvResult>;

public sealed record BulkCsvResult(int Total, int Inserted, IReadOnlyList<long> PaymentIds);
