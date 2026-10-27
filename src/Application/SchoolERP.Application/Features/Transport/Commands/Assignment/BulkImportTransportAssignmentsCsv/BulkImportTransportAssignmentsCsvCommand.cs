using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.Assignment;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.Assignment.BulkImportTransportAssignmentsCsv;

public record BulkImportTransportAssignmentsCsvCommand(BulkTransportAssignmentCsvRequest Request) : ICommand<BulkImportTransportAssignmentsResult>;

public sealed record BulkImportTransportAssignmentsResult(int Total, int Inserted, int Skipped, IReadOnlyList<string> Errors);
