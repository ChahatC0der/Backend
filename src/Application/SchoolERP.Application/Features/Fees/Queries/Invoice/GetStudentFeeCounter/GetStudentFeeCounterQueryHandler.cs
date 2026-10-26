using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;
using SchoolERP.Domain.Shared.Results;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;
using StudentCreditLedgerEntity = SchoolERP.Domain.Fees.Entities.StudentCreditLedger;
using FeeStructureEntity = SchoolERP.Domain.Fees.Entities.FeeStructure;
using StudentFeeAssignmentEntity = SchoolERP.Domain.Fees.Entities.StudentFeeAssignment;

namespace SchoolERP.Application.Features.Fees.Queries.Invoice.GetStudentFeeCounter;

public sealed class GetStudentFeeCounterQueryHandler : IRequestHandler<GetStudentFeeCounterQuery, Result<StudentFeeCounterResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentAcademicYearProvider _ay;

    public GetStudentFeeCounterQueryHandler(
        IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch,
        ICurrentAcademicYearProvider ay)
    { _db = db; _tenant = tenant; _branch = branch; _ay = ay; }

    public async Task<Result<StudentFeeCounterResponse>> Handle(GetStudentFeeCounterQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        // AY resolution
        long ayId;
        string ayName;
        if (request.AcademicYearId.HasValue)
        {
            var ay = await _ay.GetByIdAsync(request.AcademicYearId.Value, ct);
            if (ay is null) return Error.NotFound("AcademicYear", request.AcademicYearId.Value.ToString());
            ayId = ay.Id; ayName = ay.Name;
        }
        else
        {
            var current = await _ay.GetCurrentAsync(ct);
            if (current is null) return Error.Validation("No current academic year is configured for this branch.");
            ayId = current.Id; ayName = current.Name;
        }

        // Student + enrollment
        var student = await _db.Set<StudentEntity>().AsNoTracking()
            .Where(s => s.Id == request.StudentId)
            .Select(s => new { s.Id, s.FirstName, s.LastName, s.EnrollmentId, s.FamilyId })
            .FirstOrDefaultAsync(ct);
        if (student is null) return Error.NotFound("Student", request.StudentId.ToString());

        var enrollment = await _db.Set<StudentEnrollmentEntity>().AsNoTracking()
            .Where(e => e.StudentId == request.StudentId && e.AcademicYearId == ayId)
            .OrderByDescending(e => e.Id)
            .Select(e => new { e.ClassId, e.SectionId })
            .FirstOrDefaultAsync(ct);
        if (enrollment is null)
            return Error.Validation($"Student {request.StudentId} has no enrollment for the current academic year.");

        // Wallet balance: TOP 1 BalanceAfter ORDER BY Id DESC
        var walletBalance = await _db.Set<StudentCreditLedgerEntity>().AsNoTracking()
            .Where(l => l.StudentId == request.StudentId && l.TenantId == tenantId)
            .OrderByDescending(l => l.Id)
            .Select(l => (decimal?)l.BalanceAfter)
            .FirstOrDefaultAsync(ct) ?? 0m;

        // Structure resolution: override -> section -> class -> group
        long? structureId = null;
        string? structureName = null;
        var resolvedFrom = "none";

        var overrideId = await _db.Set<StudentFeeAssignmentEntity>().AsNoTracking()
            .Where(a => a.StudentId == request.StudentId && a.AcademicYearId == ayId && a.IsActive)
            .Select(a => (long?)a.FeeStructureId)
            .FirstOrDefaultAsync(ct);

        if (overrideId.HasValue)
        {
            var s = await _db.Set<FeeStructureEntity>().AsNoTracking()
                .Where(x => x.Id == overrideId.Value && x.DeletedAt == null)
                .Select(x => new { x.Id, x.Name })
                .FirstOrDefaultAsync(ct);
            if (s is not null) { structureId = s.Id; structureName = s.Name; resolvedFrom = "override"; }
        }

        if (structureId is null)
        {
            var candidates = await _db.Set<FeeStructureEntity>().AsNoTracking()
                .Where(x => x.BranchId == branchId && x.AcademicYearId == ayId
                         && x.DeletedAt == null && x.Status == FeeStructureStatus.Active)
                .Select(x => new { x.Id, x.Name, x.ClassId, x.SectionId, x.ClassGroupId })
                .ToListAsync(ct);

            var bySection = candidates.FirstOrDefault(x => x.ClassId == enrollment.ClassId && x.SectionId == enrollment.SectionId);
            var byClass = candidates.FirstOrDefault(x => x.ClassId == enrollment.ClassId && x.SectionId == null);
            var byGroup = candidates.FirstOrDefault(x => x.ClassGroupId != null && x.ClassId == null && x.SectionId == null);

            if (bySection is not null) { structureId = bySection.Id; structureName = bySection.Name; resolvedFrom = "section"; }
            else if (byClass is not null) { structureId = byClass.Id; structureName = byClass.Name; resolvedFrom = "class"; }
            else if (byGroup is not null) { structureId = byGroup.Id; structureName = byGroup.Name; resolvedFrom = "classgroup"; }
        }

        // Pending + partially paid + overdue invoices with lines
        var invoices = await _db.Set<InvoiceEntity>().AsNoTracking()
            .Include(i => i.Lines)
            .Where(i => i.TenantId == tenantId && i.BranchId == branchId
                     && i.StudentId == request.StudentId && i.AcademicYearId == ayId
                     && i.DeletedAt == null
                     && (i.Status == InvoiceStatus.Draft
                         || i.Status == InvoiceStatus.Sent
                         || i.Status == InvoiceStatus.PartiallyPaid
                         || i.Status == InvoiceStatus.Overdue))
            .OrderBy(i => i.DueDate)
            .ToListAsync(ct);

        var invoiceResponses = invoices.Select(InvoiceResponseFactory.ToResponse).ToList();
        var totalOutstanding = Money.Round(invoices.Sum(i => i.NetPayable - (i.PaidAmount + i.CreditAppliedAmount)));

        return Result.Success(new StudentFeeCounterResponse
        {
            StudentId = student.Id,
            StudentName = $"{student.FirstName} {student.LastName}".Trim(),
            EnrollmentId = student.EnrollmentId,
            ClassId = enrollment.ClassId,
            SectionId = enrollment.SectionId,
            FamilyId = student.FamilyId,
            AcademicYearId = ayId,
            AcademicYearName = ayName,
            FeeStructureId = structureId,
            StructureName = structureName,
            ResolvedFrom = resolvedFrom,
            WalletBalance = walletBalance,
            TotalOutstanding = totalOutstanding,
            PendingInvoices = invoiceResponses,
        });
    }
}
