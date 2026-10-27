using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Fees.DTOs.Portal;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;
using SchoolERP.Domain.Shared.Results;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;
using PaymentEntity = SchoolERP.Domain.Fees.Entities.Payment;
using StudentCreditLedgerEntity = SchoolERP.Domain.Fees.Entities.StudentCreditLedger;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Fees.Queries.Portal.GetParentChildFeeView;

public sealed class GetParentChildFeeViewQueryHandler : IRequestHandler<GetParentChildFeeViewQuery, Result<ParentChildFeeViewResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentUserService _user;
    private readonly ICurrentAcademicYearProvider _ay;

    public GetParentChildFeeViewQueryHandler(
        IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch,
        ICurrentUserService user, ICurrentAcademicYearProvider ay)
    { _db = db; _tenant = tenant; _branch = branch; _user = user; _ay = ay; }

    public async Task<Result<ParentChildFeeViewResponse>> Handle(GetParentChildFeeViewQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        // AUTH: current user must be parent of this student
        var currentUserId = _user.GetUserId() ?? 0;
        var auth = await ParentChildResolver.EnsureParentAsync(_db, currentUserId, request.StudentId, ct);
        if (auth.IsFailure) return auth.Error!;

        // AY resolution
        long ayId; string ayName;
        if (request.AcademicYearId.HasValue)
        {
            var a = await _ay.GetByIdAsync(request.AcademicYearId.Value, ct);
            if (a is null) return Error.NotFound("AcademicYear", request.AcademicYearId.Value.ToString());
            ayId = a.Id; ayName = a.Name;
        }
        else
        {
            var a = await _ay.GetCurrentAsync(ct);
            if (a is null) return Error.Validation("No current academic year is configured for this branch.");
            ayId = a.Id; ayName = a.Name;
        }

        // Student
        var student = await _db.Set<StudentEntity>().AsNoTracking()
            .Where(s => s.Id == request.StudentId)
            .Select(s => new { s.Id, s.FirstName, s.LastName, s.EnrollmentId })
            .FirstOrDefaultAsync(ct);
        if (student is null) return Error.NotFound("Student", request.StudentId.ToString());

        // Wallet balance
        var wallet = await _db.Set<StudentCreditLedgerEntity>().AsNoTracking()
            .Where(l => l.StudentId == request.StudentId && l.TenantId == tenantId)
            .OrderByDescending(l => l.Id)
            .Select(l => (decimal?)l.BalanceAfter)
            .FirstOrDefaultAsync(ct) ?? 0m;

        // Fee Bills (Invoices)  terminology mapped to "Bill"
        var invoices = await _db.Set<InvoiceEntity>().AsNoTracking()
            .Where(i => i.TenantId == tenantId && i.BranchId == branchId
                     && i.StudentId == request.StudentId && i.AcademicYearId == ayId
                     && i.DeletedAt == null && i.Status != InvoiceStatus.Cancelled)
            .OrderByDescending(i => i.GenerationDate)
            .Select(i => new
            {
                i.Id, i.InvoiceNumber, i.PeriodLabel, i.GenerationDate, i.DueDate,
                i.TotalAmount, i.ConcessionAmount, i.LateFeeAmount, i.NetPayable,
                i.PaidAmount, i.CreditAppliedAmount, i.Status,
            })
            .ToListAsync(ct);

        var bills = invoices.Select(i => new ParentFeeBillDto
        {
            BillId = i.Id,
            BillNumber = i.InvoiceNumber,
            PeriodLabel = i.PeriodLabel,
            BillDate = i.GenerationDate,
            DueDate = i.DueDate,
            TotalAmount = i.TotalAmount,
            ConcessionAmount = i.ConcessionAmount,
            LateFeeAmount = i.LateFeeAmount,
            NetPayable = i.NetPayable,
            PaidAmount = i.PaidAmount,
            WalletApplied = i.CreditAppliedAmount,
            Outstanding = Money.Round(i.NetPayable - i.PaidAmount - i.CreditAppliedAmount),
            Status = i.Status.ToString(),
        }).ToList();

        var totalOutstanding = Money.Round(bills.Sum(b => b.Outstanding));

        // Fee Receipts (Payments)  terminology mapped to "Receipt"
        var payments = await _db.Set<PaymentEntity>().AsNoTracking()
            .Where(p => p.TenantId == tenantId && p.BranchId == branchId
                     && p.StudentId == request.StudentId
                     && p.DeletedAt == null)
            .OrderByDescending(p => p.PaymentDate)
            .Take(50)
            .Select(p => new ParentFeeReceiptDto
            {
                ReceiptId = p.Id,
                ReceiptNumber = p.ReceiptNumber,
                ReceiptDate = p.PaymentDate,
                Amount = p.Amount,
                Mode = p.Mode.ToString(),
                Status = p.Status.ToString(),
            })
            .ToListAsync(ct);

        return Result.Success(new ParentChildFeeViewResponse
        {
            ChildId = student.Id,
            ChildName = $"{student.FirstName} {student.LastName}".Trim(),
            EnrollmentId = student.EnrollmentId,
            AcademicYearId = ayId,
            AcademicYearName = ayName,
            AdvanceWalletBalance = wallet,
            TotalOutstanding = totalOutstanding,
            FeeBills = bills,
            FeeReceipts = payments,
        });
    }
}
