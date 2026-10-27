using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.DTOs.Assignment;
using SchoolERP.Domain.Shared.Results;
using TransportAssignmentEntity = SchoolERP.Domain.Transport.Entities.TransportAssignment;

namespace SchoolERP.Application.Features.Transport.Commands.Assignment.BulkImportTransportAssignmentsCsv;

/// <summary>All-or-nothing bulk import. Cap 500. Validates all rows before any mutation.</summary>
public sealed class BulkImportTransportAssignmentsCsvCommandHandler : IRequestHandler<BulkImportTransportAssignmentsCsvCommand, Result<BulkImportTransportAssignmentsResult>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public BulkImportTransportAssignmentsCsvCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    { _db = db; _tenant = tenant; }

    public async Task<Result<BulkImportTransportAssignmentsResult>> Handle(BulkImportTransportAssignmentsCsvCommand request, CancellationToken ct)
    {
        var tenantId = _tenant.GetTenantId();
        if (tenantId == Guid.Empty) return Error.Unauthorized("Tenant context is not resolved.");

        var r = request.Request;
        if (r.Rows.Count > 500)
            return Error.Validation("Bulk import is capped at 500 rows per request in Phase 1.");

        // --- Phase 1: validate ALL rows before any mutation ---
        var rowErrors = new List<string>();
        var prepared = new List<TransportAssignmentEntity>();
        var seenStudents = new HashSet<long>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        for (int i = 0; i < r.Rows.Count; i++)
        {
            var row = r.Rows[i];
            var rowNum = i + 1;

            if (!seenStudents.Add(row.StudentId))
            {
                rowErrors.Add($"Row {rowNum}: duplicate student {row.StudentId} in this batch.");
                continue;
            }

            var v = await AssignmentValidator.ValidateAsync(_db, tenantId, row.StudentId, r.AcademicYearId,
                row.PickupVehicleId, row.PickupRouteId, row.PickupStopId,
                row.DropVehicleId, row.DropRouteId, row.DropStopId, ct);
            if (v.IsFailure)
            {
                rowErrors.Add($"Row {rowNum}: {v.Error!.ToString()}");
                continue;
            }

            var exists = await _db.Set<TransportAssignmentEntity>().AnyAsync(a =>
                a.TenantId == tenantId && a.StudentId == row.StudentId && a.AcademicYearId == r.AcademicYearId
                && a.IsCurrent && !a.IsDeleted, ct);
            if (exists)
            {
                rowErrors.Add($"Row {rowNum}: student {row.StudentId} already has a current assignment for AY {r.AcademicYearId}.");
                continue;
            }

            prepared.Add(new TransportAssignmentEntity
            {
                TenantId = tenantId,
                StudentId = row.StudentId,
                AcademicYearId = r.AcademicYearId,
                PickupVehicleId = row.PickupVehicleId,
                PickupRouteId = row.PickupRouteId,
                PickupStopId = row.PickupStopId,
                DropVehicleId = row.DropVehicleId,
                DropRouteId = row.DropRouteId,
                DropStopId = row.DropStopId,
                ValidFrom = row.ValidFrom ?? today,
                IsCurrent = true,
                CreatedAt = DateTime.UtcNow,
            });
        }

        if (rowErrors.Count > 0)
        {
            var preview = string.Join(" | ", rowErrors.Take(5));
            var more = rowErrors.Count > 5 ? $" (+{rowErrors.Count - 5} more)" : "";
            return Error.Validation($"Bulk import rejected (all-or-nothing). Failures: {preview}{more}");
        }

        // --- Phase 2: write all ---
        _db.Set<TransportAssignmentEntity>().AddRange(prepared);
        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("TransportAssignments", 0, FinancialAuditAction.Create, null, new { Count = prepared.Count, AcademicYearId = r.AcademicYearId }, ct);

        return Result.Success(new BulkImportTransportAssignmentsResult(
            Total: r.Rows.Count,
            Inserted: prepared.Count,
            Skipped: 0,
            Errors: Array.Empty<string>()));
    }
}
