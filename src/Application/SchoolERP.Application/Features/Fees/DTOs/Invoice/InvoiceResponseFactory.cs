using SchoolERP.Domain.Fees.ValueObjects;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;
using InvoiceLineEntity = SchoolERP.Domain.Fees.Entities.InvoiceLine;

namespace SchoolERP.Application.Features.Fees.DTOs.Invoice;

/// <summary>Single mapping point entity → response. Used by all invoice handlers.</summary>
public static class InvoiceResponseFactory
{
    public static InvoiceResponse ToResponse(InvoiceEntity e)
    {
        var outstanding = Money.Round(e.NetPayable - (e.PaidAmount + e.CreditAppliedAmount));
        return new InvoiceResponse
        {
            Id = e.Id,
            StudentId = e.StudentId,
            AcademicYearId = e.AcademicYearId,
            ClassId = e.ClassId,
            SectionId = e.SectionId,
            FeeStructureId = e.FeeStructureId,
            InvoiceNumber = e.InvoiceNumber,
            PeriodLabel = e.PeriodLabel,
            GenerationDate = e.GenerationDate,
            DueDate = e.DueDate,
            TotalAmount = e.TotalAmount,
            ConcessionAmount = e.ConcessionAmount,
            LateFeeAmount = e.LateFeeAmount,
            NetPayable = e.NetPayable,
            PaidAmount = e.PaidAmount,
            CreditAppliedAmount = e.CreditAppliedAmount,
            Outstanding = outstanding,
            Status = e.Status.ToString(),
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            Lines = e.Lines.Select(ToLineResponse).ToList(),
        };
    }

    public static InvoiceLineResponse ToLineResponse(InvoiceLineEntity l) => new()
    {
        Id = l.Id,
        FeeHeadId = l.FeeHeadId,
        FeeHeadName = l.FeeHeadNameSnapshot,
        Amount = l.Amount,
        ConcessionApplied = l.ConcessionApplied,
        NetAmount = l.NetAmount,
        IsLateFeeLine = l.IsLateFeeLine,
        SourceInvoiceLineId = l.SourceInvoiceLineId,
    };

    public static InvoiceListResponse ToListResponse(InvoiceEntity e) => new()
    {
        Id = e.Id,
        StudentId = e.StudentId,
        AcademicYearId = e.AcademicYearId,
        ClassId = e.ClassId,
        SectionId = e.SectionId,
        InvoiceNumber = e.InvoiceNumber,
        PeriodLabel = e.PeriodLabel,
        DueDate = e.DueDate,
        TotalAmount = e.TotalAmount,
        NetPayable = e.NetPayable,
        PaidAmount = e.PaidAmount,
        CreditAppliedAmount = e.CreditAppliedAmount,
        Outstanding = Money.Round(e.NetPayable - (e.PaidAmount + e.CreditAppliedAmount)),
        Status = e.Status.ToString(),
        CreatedAt = e.CreatedAt,
    };
}
