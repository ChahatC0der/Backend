using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.Payment;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;
using SchoolERP.Domain.Shared.Results;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;
using PaymentEntity = SchoolERP.Domain.Fees.Entities.Payment;
using PaymentAllocationEntity = SchoolERP.Domain.Fees.Entities.PaymentAllocation;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Two-phase payment collection. Used by CollectPayment AND BulkRecordPaymentsCsv.
/// Phase 1 (BuildAsync): validate everything, insert Payment + Allocations, no wallet.
/// Phase 2 (FinalizeAsync): caller must call db.SaveChangesAsync between phases (to populate
///                         Payment.Id), then call FinalizeAsync to update invoices + apply wallet.
/// </summary>
public static class PaymentCollector
{
    public sealed record BuildResult(PaymentEntity Payment, IReadOnlyList<InvoiceEntity> Invoices);

    public static async Task<Result<BuildResult>> BuildAsync(
        IApplicationDbContext db,
        IFeeSequenceGenerator sequence,
        long? currentUserId,
        Guid tenantId, Guid branchId, string ayCode,
        CollectPaymentRequest request,
        CancellationToken ct)
    {
        if (!TryParseMode(request.Mode, out var mode))
            return Error.Validation($"Invalid payment mode: {request.Mode}.");

        var studentExists = await db.Set<StudentEntity>().AsNoTracking()
            .AnyAsync(s => s.Id == request.StudentId, ct);
        if (!studentExists) return Error.NotFound("Student", request.StudentId.ToString());

        var allocatedSum = request.Allocations.Sum(a => a.AmountToAllocate);
        if (allocatedSum > request.TotalAmountReceived)
            return Error.Validation($"Allocated {allocatedSum} exceeds total received {request.TotalAmountReceived}.");

        var invoiceIds = request.Allocations.Select(a => a.InvoiceId).Distinct().ToList();
        var invoices = new List<InvoiceEntity>();
        if (invoiceIds.Count > 0)
        {
            invoices = await db.Set<InvoiceEntity>()
                .Where(i => invoiceIds.Contains(i.Id)
                         && i.TenantId == tenantId
                         && i.BranchId == branchId
                         && i.DeletedAt == null)
                .ToListAsync(ct);

            if (invoices.Count != invoiceIds.Count)
                return Error.Validation("One or more allocations reference missing or cross-tenant invoices.");
        }

        foreach (var alloc in request.Allocations)
        {
            var invoice = invoices.First(i => i.Id == alloc.InvoiceId);
            if (invoice.StudentId != request.StudentId)
                return Error.Validation($"Invoice {alloc.InvoiceId} belongs to a different student.");
            if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled)
                return Error.Conflict($"Invoice {alloc.InvoiceId} is {invoice.Status}.");
            var outstanding = Money.Round(invoice.NetPayable - (invoice.PaidAmount + invoice.CreditAppliedAmount));
            if (alloc.AmountToAllocate > outstanding)
                return Error.Validation($"Invoice {alloc.InvoiceId}: allocation {alloc.AmountToAllocate} exceeds outstanding {outstanding}.");
        }

        var allocatedAmount = Money.Round(allocatedSum);
        var unallocatedAmount = Money.Round(request.TotalAmountReceived - allocatedAmount);

        var receiptNumber = await sequence.NextReceiptNumberAsync(branchId, ayCode, ct);

        var payment = new PaymentEntity
        {
            TenantId = tenantId,
            BranchId = branchId,
            StudentId = request.StudentId,
            ReceiptNumber = receiptNumber,
            PaymentDate = request.PaymentDate == default ? DateTime.UtcNow : request.PaymentDate,
            Amount = request.TotalAmountReceived,
            AllocatedAmount = allocatedAmount,
            UnallocatedAmount = unallocatedAmount,
            PaymentSource = PaymentSource.Counter,
            Mode = mode,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
            CollectedBy = currentUserId,
            Status = mode == PaymentMode.Cheque ? PaymentStatus.Pending : PaymentStatus.Completed,
            CreatedAt = DateTime.UtcNow,
        };

        foreach (var alloc in request.Allocations)
        {
            payment.Allocations.Add(new PaymentAllocationEntity
            {
                InvoiceId = alloc.InvoiceId,
                AllocatedAmount = alloc.AmountToAllocate,
                CreatedAt = DateTime.UtcNow,
            });
        }

        db.Set<PaymentEntity>().Add(payment);
        return Result.Success(new BuildResult(payment, invoices));
    }

    public static async Task<Result> FinalizeAsync(
        IApplicationDbContext db,
        IWalletLedgerWriter wallet,
        BuildResult built,
        CancellationToken ct)
    {
        foreach (var alloc in built.Payment.Allocations)
        {
            var invoice = built.Invoices.First(i => i.Id == alloc.InvoiceId);
            invoice.PaidAmount = Money.Round(invoice.PaidAmount + alloc.AllocatedAmount);
            invoice.Status = invoice.PaidAmount + invoice.CreditAppliedAmount >= invoice.NetPayable
                ? InvoiceStatus.Paid
                : InvoiceStatus.PartiallyPaid;
            invoice.UpdatedAt = DateTime.UtcNow;
        }

        if (built.Payment.UnallocatedAmount > 0)
        {
            var walletResult = await wallet.ApplyAsync(new WalletOperation(
                StudentId: built.Payment.StudentId,
                TenantId: built.Payment.TenantId,
                AcademicYearId: null,
                Delta: built.Payment.UnallocatedAmount,
                Type: CreditTransactionType.AdvancePayment,
                ReferencePaymentId: built.Payment.Id,
                Reason: "Unallocated excess from collection"), ct);
            if (walletResult.IsFailure) return Result.Failure(walletResult.Error!);
        }

        return Result.Success(true);
    }

    internal static bool TryParseMode(string value, out PaymentMode mode)
    {
        foreach (var kv in EnumMaps_JsonAdapter.PaymentModeMap)
            if (string.Equals(kv.Value, value, StringComparison.OrdinalIgnoreCase)) { mode = kv.Key; return true; }
        mode = default; return false;
    }
}
