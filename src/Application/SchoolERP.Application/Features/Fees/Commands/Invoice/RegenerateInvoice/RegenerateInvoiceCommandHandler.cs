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
using ConcessionUsageEntity = SchoolERP.Domain.Fees.Entities.ConcessionUsage;

namespace SchoolERP.Application.Features.Fees.Commands.Invoice.RegenerateInvoice;

public sealed class RegenerateInvoiceCommandHandler : IRequestHandler<RegenerateInvoiceCommand, Result<InvoiceResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentAcademicYearProvider _ay;
    private readonly IFeeSequenceGenerator _sequence;
    private readonly ITransportFeeQuoteProvider _transportProvider;

    public RegenerateInvoiceCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch,
        ICurrentAcademicYearProvider ay, IFeeSequenceGenerator sequence,
        ITransportFeeQuoteProvider transportProvider)
    {
        _db = db; _tenant = tenant; _branch = branch;
        _ay = ay; _sequence = sequence; _transportProvider = transportProvider;
    }

    public async Task<Result<InvoiceResponse>> Handle(RegenerateInvoiceCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var old = await _db.Set<InvoiceEntity>().Include(i => i.Lines).FirstOrDefaultAsync(i =>
            i.Id == request.InvoiceId && i.TenantId == tenantId
            && i.BranchId == branchId && i.DeletedAt == null, ct);
        if (old is null) return Error.NotFound("Invoice", request.InvoiceId.ToString());

        if (old.Status == InvoiceStatus.Paid)
            return Error.Conflict("Cannot regenerate a paid invoice.");
        if (old.Status == InvoiceStatus.Cancelled)
            return Error.Conflict("Cannot regenerate a cancelled invoice.");

        var ayInfo = await _ay.GetByIdAsync(old.AcademicYearId, ct);
        if (ayInfo is null) return Error.NotFound("AcademicYear", old.AcademicYearId.ToString());

        var draftResult = await InvoiceBuilder.BuildAsync(_db, old.StudentId, old.AcademicYearId,
            old.PeriodLabel, DateTime.UtcNow, old.DueDate, _transportProvider, ct);
        if (draftResult.IsFailure) return draftResult.Error!;
        var draft = draftResult.Value;

        old.Status = InvoiceStatus.Cancelled;
        old.UpdatedAt = DateTime.UtcNow;

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

        return Result.Success(InvoiceResponseFactory.ToResponse(invoice));
    }
}
