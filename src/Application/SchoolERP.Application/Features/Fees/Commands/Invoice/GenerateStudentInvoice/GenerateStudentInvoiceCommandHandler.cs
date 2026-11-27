using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using ConcessionUsageEntity = SchoolERP.Domain.Fees.Entities.ConcessionUsage;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;
using InvoiceLineEntity = SchoolERP.Domain.Fees.Entities.InvoiceLine;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.GenerateStudentInvoice;

public sealed class GenerateStudentInvoiceCommandHandler
    : IRequestHandler<GenerateStudentInvoiceCommand, Result<IReadOnlyList<InvoiceResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentAcademicYearProvider _ay;
    private readonly IFeeSequenceGenerator _sequence;

    public GenerateStudentInvoiceCommandHandler(
        IApplicationDbContext db,
        ICurrentTenantService tenant,
        ICurrentBranchService branch,
        ICurrentAcademicYearProvider ay,
        IFeeSequenceGenerator sequence)
    {
        _db = db;
        _tenant = tenant;
        _branch = branch;
        _ay = ay;
        _sequence = sequence;
    }

    public async Task<Result<IReadOnlyList<InvoiceResponse>>> Handle(
        GenerateStudentInvoiceCommand request,
        CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var req = request.Request;
        if (req.StudentId <= 0 || req.AcademicYearId <= 0)
            return Error.Validation("StudentId and AcademicYearId must be positive.");

        if (req.PeriodSequenceNumbers is null || req.PeriodSequenceNumbers.Count == 0)
            return Error.Validation("At least one period sequence number is required.");

        var ayInfo = await _ay.GetByIdAsync(req.AcademicYearId, ct);
        if (ayInfo is null)
            return Error.NotFound("AcademicYear", req.AcademicYearId.ToString());

        var frequency = InvoiceGenerationFrequencyResolver.Resolve(tenantId);
        var billingPeriods = BillingPeriodCalculator.Calculate(
            ayInfo.StartDate,
            ayInfo.EndDate,
            frequency);

        var periodBySeq = billingPeriods.ToDictionary(p => p.SequenceNumber);
        var orderedSequences = req.PeriodSequenceNumbers.Distinct().OrderBy(x => x).ToList();

        var targetPeriods = new List<BillingPeriod>(orderedSequences.Count);
        foreach (var seq in orderedSequences)
        {
            if (!periodBySeq.TryGetValue(seq, out var period))
            {
                return Error.Validation(
                    $"Invalid period sequence number {seq}. " +
                    $"Frequency {frequency} allows {billingPeriods.Count} periods " +
                    $"in this Academic Year.");
            }

            targetPeriods.Add(period);
        }

        // Hamesha ascending sequence order mein process karo (e.g., 4 pehle, 5 baad mein)
        targetPeriods = targetPeriods
            .OrderBy(p => p.SequenceNumber)
            .ToList();

        var generatedSequences = (await _db.Set<InvoiceEntity>()
            .Where(i =>
                i.TenantId == tenantId &&
                i.BranchId == branchId &&
                i.StudentId == req.StudentId &&
                i.AcademicYearId == req.AcademicYearId &&
                i.Status != InvoiceStatus.Cancelled &&
                i.DeletedAt == null &&
                i.PeriodSequenceNumber.HasValue)
            .Select(i => i.PeriodSequenceNumber!.Value)
            .ToListAsync(ct)).ToHashSet();

        foreach (var period in targetPeriods)
        {
            var missingPreviousPeriods = billingPeriods
                .Where(p =>
                    p.SequenceNumber < period.SequenceNumber &&
                    !generatedSequences.Contains(p.SequenceNumber))
                .Select(p => p.SequenceNumber)
                .ToList();

            if (missingPreviousPeriods.Count > 0)
            {
                return Error.Validation(
                    $"Cannot generate period {period.SequenceNumber}. " +
                    $"Generate previous period(s) first: " +
                    $"{string.Join(", ", missingPreviousPeriods)}.");
            }

            if (generatedSequences.Contains(period.SequenceNumber))
            {
                var label = PeriodLabelBuilder.ForBillingPeriod(period);
                return Error.Conflict(
                    $"Invoice already exists for student {req.StudentId} " +
                    $"for period #{period.SequenceNumber} ({label}).");
            }
        }

        var createdInvoices = new List<InvoiceResponse>(targetPeriods.Count);
        var generationDate = DateTime.UtcNow;

        await _db.BeginTransactionAsync(ct);
        try
        {
            foreach (var billingPeriod in targetPeriods)
            {
                var periodLabel = PeriodLabelBuilder.ForBillingPeriod(billingPeriod);

                var draftResult = await InvoiceBuilder.BuildForPeriodAsync(
                    db: _db,
                    branchId: branchId,
                    studentId: req.StudentId,
                    academicYearId: req.AcademicYearId,
                    academicYearStartDate: ayInfo.StartDate,
                    academicYearEndDate: ayInfo.EndDate,
                    billingPeriod: billingPeriod,
                    periodLabel: periodLabel,
                    generationDate: generationDate,
                    ct: ct);

                if (draftResult.IsFailure)
                {
                    await _db.RollbackTransactionAsync(CancellationToken.None);
                    return draftResult.Error!;
                }

                var draft = draftResult.Value;
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
                    PeriodSequenceNumber = billingPeriod.SequenceNumber,
                    PeriodStartDate = billingPeriod.StartDate,
                    PeriodEndDate = billingPeriod.EndDate,
                    GenerationDate = draft.GenerationDate,
                    DueDate = draft.DueDate,
                    TotalAmount = draft.TotalAmount,
                    ConcessionAmount = draft.ConcessionAmount,
                    LateFeeAmount = 0m,
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

                _db.Set<InvoiceEntity>().Add(invoice);
                await _db.SaveChangesAsync(ct);

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

                generatedSequences.Add(billingPeriod.SequenceNumber);
                createdInvoices.Add(InvoiceResponseFactory.ToResponse(invoice));
            }

            await _db.CommitTransactionAsync(ct);
            return Result.Success<IReadOnlyList<InvoiceResponse>>(createdInvoices);
        }
        catch
        {
            await _db.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }
}