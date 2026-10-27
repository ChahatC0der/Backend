using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;
using InvoiceLineEntity = SchoolERP.Domain.Fees.Entities.InvoiceLine;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.BulkGenerateInvoices;

public sealed class BulkGenerateInvoicesCommandHandler : IRequestHandler<BulkGenerateInvoicesCommand, Result<BulkGenerateResult>>
{
    private const int MaxStudentsPerRequest = 500;

    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentAcademicYearProvider _ay;
    private readonly IFeeSequenceGenerator _sequence;
    private readonly ITransportFeeQuoteProvider _transportProvider;

    public BulkGenerateInvoicesCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch,
        ICurrentAcademicYearProvider ay, IFeeSequenceGenerator sequence,
        ITransportFeeQuoteProvider transportProvider)
    {
        _db = db; _tenant = tenant; _branch = branch;
        _ay = ay; _sequence = sequence; _transportProvider = transportProvider;
    }

    public async Task<Result<BulkGenerateResult>> Handle(BulkGenerateInvoicesCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var ayInfo = await _ay.GetByIdAsync(request.Request.AcademicYearId, ct);
        if (ayInfo is null) return Error.NotFound("AcademicYear", request.Request.AcademicYearId.ToString());

        var enrollQuery = _db.Set<StudentEnrollmentEntity>().AsNoTracking()
            .Where(e => e.AcademicYearId == request.Request.AcademicYearId);

        if (request.Request.ClassId.HasValue)
        {
            enrollQuery = enrollQuery.Where(e => e.ClassId == request.Request.ClassId.Value);
            if (request.Request.SectionId.HasValue)
                enrollQuery = enrollQuery.Where(e => e.SectionId == request.Request.SectionId.Value);
        }

        var studentIds = await enrollQuery.Select(e => e.StudentId).Distinct()
            .Take(MaxStudentsPerRequest + 1).ToListAsync(ct);
        if (studentIds.Count > MaxStudentsPerRequest)
            return Error.Validation($"Bulk generation is capped at {MaxStudentsPerRequest} students per request in Phase 1.");

        if (studentIds.Count == 0)
            return Result.Success(new BulkGenerateResult(0, 0, 0, 0, Array.Empty<string>()));

        var existing = await _db.Set<InvoiceEntity>().AsNoTracking()
            .Where(i => i.TenantId == tenantId && i.BranchId == branchId &&
                        i.AcademicYearId == request.Request.AcademicYearId &&
                        i.PeriodLabel == request.Request.PeriodLabel &&
                        i.Status != InvoiceStatus.Cancelled &&
                        i.DeletedAt == null &&
                        studentIds.Contains(i.StudentId))
            .Select(i => i.StudentId).ToListAsync(ct);
        var existingSet = existing.ToHashSet();

        var now = DateTime.UtcNow;
        var dueDate = request.Request.DueDate ?? now.AddDays(15);
        var skipped = 0;
        var generated = 0;
        var failed = 0;
        var errors = new List<string>();

        foreach (var sid in studentIds)
        {
            if (existingSet.Contains(sid)) { skipped++; continue; }

            var draftResult = await InvoiceBuilder.BuildAsync(_db, sid, request.Request.AcademicYearId,
                request.Request.PeriodLabel.Trim(), now, dueDate, _transportProvider, ct);
            if (draftResult.IsFailure)
            {
                failed++;
                errors.Add($"Student {sid}: {draftResult.Error!.ToString()}");
                continue;
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
                GenerationDate = draft.GenerationDate,
                DueDate = draft.DueDate,
                TotalAmount = draft.TotalAmount,
                ConcessionAmount = draft.ConcessionAmount,
                LateFeeAmount = 0m,
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
            generated++;
        }

        if (generated > 0)
            await _db.SaveChangesAsync(ct);

        return Result.Success(new BulkGenerateResult(studentIds.Count, generated, skipped, failed, errors));
    }
}
