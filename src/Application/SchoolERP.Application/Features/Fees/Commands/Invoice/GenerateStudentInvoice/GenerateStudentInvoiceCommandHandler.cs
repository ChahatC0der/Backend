using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;
using SchoolERP.Domain.Shared.Results;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;
using InvoiceLineEntity = SchoolERP.Domain.Fees.Entities.InvoiceLine;
using ConcessionUsageEntity = SchoolERP.Domain.Fees.Entities.ConcessionUsage;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.GenerateStudentInvoice;

public sealed class GenerateStudentInvoiceCommandHandler : IRequestHandler<GenerateStudentInvoiceCommand, Result<InvoiceResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentAcademicYearProvider _ay;
    private readonly IFeeSequenceGenerator _sequence;

    public GenerateStudentInvoiceCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch,
        ICurrentAcademicYearProvider ay, IFeeSequenceGenerator sequence)
    { _db = db; _tenant = tenant; _branch = branch; _ay = ay; _sequence = sequence; }

    public async Task<Result<InvoiceResponse>> Handle(GenerateStudentInvoiceCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        // --- All validations BEFORE any mutation ---
        var ayInfo = await _ay.GetByIdAsync(request.Request.AcademicYearId, ct);
        if (ayInfo is null) return Error.NotFound("AcademicYear", request.Request.AcademicYearId.ToString());

        // ---------------------------------------------------------
        // BLOCK: Resolve tenant invoice frequency and billing period.
        //
        // The tenant frequency determines how many billing periods
        // exist in the Academic Year.
        // BillingPeriodCalculator calculates dates from AY boundaries.
        // ---------------------------------------------------------

        var frequency = InvoiceGenerationFrequencyResolver.Resolve(tenantId);

        var billingPeriods = BillingPeriodCalculator.Calculate(
            ayInfo.StartDate,
            ayInfo.EndDate,
            frequency);

        // Find the exact requested period.
        var billingPeriod = billingPeriods.FirstOrDefault(
            p => p.SequenceNumber == request.Request.PeriodSequenceNumber);

        // Reject a sequence number outside the Academic Year's periods.
        if (billingPeriod is null)
        {
            return Error.Validation(
                $"Invalid period sequence number. " +
                $"Frequency {frequency} allows {billingPeriods.Count} periods " +
                $"in this Academic Year.");
        }

        // ---------------------------------------------------------
        // PeriodLabel is DERIVED from the resolved billing period,
        // never taken from the request DTO.
        // ---------------------------------------------------------
        var periodLabel = PeriodLabelBuilder.ForBillingPeriod(billingPeriod);

        // ---------------------------------------------------------
        // BLOCK: Enforce sequential invoice generation.
        //
        // Every earlier billing period must have a non-cancelled
        // invoice for this student, branch and Academic Year.
        // Payment status does not matter.
        // ---------------------------------------------------------

        var generatedSequences = await _db.Set<InvoiceEntity>()
            .Where(i =>
                i.TenantId == tenantId &&
                i.BranchId == branchId &&
                i.StudentId == request.Request.StudentId &&
                i.AcademicYearId == request.Request.AcademicYearId &&
                i.Status != InvoiceStatus.Cancelled &&
                i.DeletedAt == null &&
                i.PeriodSequenceNumber.HasValue)
            .Select(i => i.PeriodSequenceNumber!.Value)
            .ToListAsync(ct);

        var missingPreviousPeriods = billingPeriods
            .Where(p =>
                p.SequenceNumber < billingPeriod.SequenceNumber &&
                !generatedSequences.Contains(p.SequenceNumber))
            .Select(p => p.SequenceNumber)
            .ToList();

        if (missingPreviousPeriods.Count > 0)
        {
            return Error.Validation(
                $"Cannot generate period {billingPeriod.SequenceNumber}. " +
                $"Generate previous period(s) first: " +
                $"{string.Join(", ", missingPreviousPeriods)}.");
        }

        // Duplicate check keyed on PeriodSequenceNumber (period identity),
        // NOT on PeriodLabel. Manual / ADMISSION invoices have a null
        // PeriodSequenceNumber, so they never collide with periodic generation.
        var duplicate = await _db.Set<InvoiceEntity>().AnyAsync(i =>
            i.TenantId == tenantId && i.BranchId == branchId &&
            i.StudentId == request.Request.StudentId &&
            i.AcademicYearId == request.Request.AcademicYearId &&
            i.PeriodSequenceNumber == billingPeriod.SequenceNumber &&
            i.Status != InvoiceStatus.Cancelled &&
            i.DeletedAt == null, ct);
        if (duplicate)
            return Error.Conflict(
                $"Invoice already exists for student {request.Request.StudentId} " +
                $"for period #{billingPeriod.SequenceNumber} ({periodLabel}).");

        var generationDate = DateTime.UtcNow;

        // ---------------------------------------------------------
        // DueDate is NOT a request input anymore.
        // The builder owns the due-date rule and returns it on the
        // draft — the handler only reads draft.DueDate when mapping
        // the entity. No DTO value, no fallback logic here.
        // ---------------------------------------------------------

        // ---------------------------------------------------------
        // BLOCK: Build invoice draft for the selected billing period.
        //
        // The period-aware builder handles:
        // - Student enrollment and eligibility.
        // - Applicable FeeStructure resolution.
        // - FeeHead frequency filtering.
        // - Concession calculations.
        // - Invoice line snapshots (per-line late fee rules).
        // - Header late fee snapshots (from FeeStructure header).
        //
        // This method only builds an in-memory draft.
        // It does not save anything to the database.
        // ---------------------------------------------------------

        var draftResult = await InvoiceBuilder.BuildForPeriodAsync(
            db: _db,
            branchId: branchId,
            studentId: request.Request.StudentId,
            academicYearId: request.Request.AcademicYearId,
            academicYearStartDate: ayInfo.StartDate,
            academicYearEndDate: ayInfo.EndDate,
            billingPeriod: billingPeriod,
            periodLabel: periodLabel,
            generationDate: generationDate,
            ct: ct);

        if (draftResult.IsFailure)
            return draftResult.Error!;

        var draft = draftResult.Value;

        // --- Mutations start here ---
        var invoiceNumber = await _sequence.NextInvoiceNumberAsync(branchId, ayInfo.Code, ct);

        var invoice = new InvoiceEntity
        {
            TenantId = tenantId,
            BranchId = branchId,
            StudentId = draft.StudentId,
            AcademicYearId = draft.AcademicYearId,
            ClassId = draft.ClassId,
            SectionId = draft.SectionId,
            FeeStructureId = draft.FeeStructureId,
            InvoiceNumber = invoiceNumber,
            PeriodLabel = draft.PeriodLabel,
            // Billing period tracking.
            PeriodSequenceNumber = billingPeriod.SequenceNumber,
            PeriodStartDate = billingPeriod.StartDate,
            PeriodEndDate = billingPeriod.EndDate,
            GenerationDate = draft.GenerationDate,
            DueDate = draft.DueDate,
            TotalAmount = draft.TotalAmount,
            ConcessionAmount = draft.ConcessionAmount,
            LateFeeAmount = 0m,

            // -----------------------------------------------------
            // Late fee header snapshots — FROZEN by the builder from
            // the FeeStructure header at generation time.
            // Birth certificate: never recomputed, never updated.
            // The LateFeeEngine reads these at payment / recalc time.
            // null LateFeeModeSnapshot = legacy per_head behaviour.
            // -----------------------------------------------------
            LateFeeModeSnapshot = draft.LateFeeModeSnapshot,
            InvoiceLateFeeTypeSnapshot = draft.InvoiceLateFeeTypeSnapshot,
            InvoiceLateFeeValueSnapshot = draft.InvoiceLateFeeValueSnapshot,
            InvoiceLateFeeGraceDaysSnapshot = draft.InvoiceLateFeeGraceDaysSnapshot,
            InvoiceLateFeeCapPercentSnapshot = draft.InvoiceLateFeeCapPercentSnapshot,
            InvoiceLateFeeSlabConfigSnapshot = draft.InvoiceLateFeeSlabConfigSnapshot,
            InvoiceLateFeeCapBase = draft.InvoiceLateFeeCapBase,

            NetPayable = draft.NetPayable,
            PaidAmount = 0m,
            CreditAppliedAmount = 0m,

            Status = InvoiceStatus.Draft,
            CreatedAt = DateTime.UtcNow,
        };

        foreach (var d in draft.Lines)
        {
            invoice.Lines.Add(new InvoiceLineEntity
            {
                FeeHeadId = d.FeeHeadId,
                SourceFeeStructureLineId = d.SourceFeeStructureLineId,
                FeeHeadNameSnapshot = d.FeeHeadNameSnapshot,
                Amount = d.Amount,
                ConcessionApplied = d.ConcessionApplied,
                NetAmount = d.NetAmount,
                IsLateFeeLine = false,
                LateFeeApplicable = d.LateFeeApplicable,
                LateFeeTypeSnapshot = d.LateFeeTypeSnapshot,
                LateFeeValueSnapshot = d.LateFeeValueSnapshot,
                LateFeeGraceDaysSnapshot = d.LateFeeGraceDaysSnapshot,
                LateFeeCapPercentSnapshot = d.LateFeeCapPercentSnapshot,
                LateFeeSlabConfigSnapshot = d.LateFeeSlabConfigSnapshot,
                LateFeeCapBase = d.LateFeeCapBase,
                CreatedAt = DateTime.UtcNow,
            });
        }

        // ---------------------------------------------------------
        // BLOCK: Persist invoice + lines + concession usages in ONE
        // transaction. Two separate SaveChangesAsync calls risked an
        // orphan invoice (invoice saved, zero usage rows) if the second
        // save failed — breaking concession dedup by ConcessionId.
        // ---------------------------------------------------------
        await _db.BeginTransactionAsync(ct);
        try
        {
            _db.Set<InvoiceEntity>().Add(invoice);
            await _db.SaveChangesAsync(ct);

            // Concession usage tracking — one row per (allocation, invoice)
            if (draft.ConcessionUsages.Count > 0)
            {
                var usages = draft.ConcessionUsages.Select(u => new ConcessionUsageEntity
                {
                    ConcessionAllocationId = u.ConcessionAllocationId,
                    InvoiceId = invoice.Id,
                    AppliedAmount = u.AppliedAmount,
                    CreatedAt = DateTime.UtcNow,
                }).ToList();
                _db.Set<ConcessionUsageEntity>().AddRange(usages);
                await _db.SaveChangesAsync(ct);
            }

            await _db.CommitTransactionAsync(ct);
        }
        catch
        {
            await _db.RollbackTransactionAsync(ct);
            throw;
        }

        // TODO (audit): await _auditWriter.LogAsync("Invoices", invoice.Id, FinancialAuditAction.Create, null, invoice, ct);

        var response = InvoiceResponseFactory.ToResponse(invoice);
        return Result.Success(response);
    }
}