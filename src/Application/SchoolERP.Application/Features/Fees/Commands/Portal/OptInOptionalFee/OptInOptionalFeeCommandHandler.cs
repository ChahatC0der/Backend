using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Fees.DTOs.Invoice;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;
using InvoiceLineEntity = SchoolERP.Domain.Fees.Entities.InvoiceLine;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;

namespace SchoolERP.Application.Features.Fees.Commands.Portal.OptInOptionalFee;

public sealed class OptInOptionalFeeCommandHandler : IRequestHandler<OptInOptionalFeeCommand, Result<InvoiceResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentUserService _user;
    private readonly ICurrentAcademicYearProvider _ay;
    private readonly IFeeSequenceGenerator _sequence;

    public OptInOptionalFeeCommandHandler(
        IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch,
        ICurrentUserService user, ICurrentAcademicYearProvider ay, IFeeSequenceGenerator sequence)
    { _db = db; _tenant = tenant; _branch = branch; _user = user; _ay = ay; _sequence = sequence; }

    public async Task<Result<InvoiceResponse>> Handle(OptInOptionalFeeCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var auth = await ParentChildResolver.EnsureParentAsync(_db, _user.GetUserId() ?? 0, request.Request.StudentId, ct);
        if (auth.IsFailure) return auth.Error!;

        var ay = await _ay.GetByIdAsync(request.Request.AcademicYearId, ct);
        if (ay is null) return Error.NotFound("AcademicYear", request.Request.AcademicYearId.ToString());

        // Fee head must be optional
        var feeHead = await _db.Set<FeeHeadEntity>().AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == request.Request.FeeHeadId
                                  && h.TenantId == tenantId && h.BranchId == branchId
                                  && h.DeletedAt == null && h.Status == FeeHeadStatus.Active, ct);
        if (feeHead is null) return Error.NotFound("FeeHead", request.Request.FeeHeadId.ToString());
        if (!feeHead.IsOptional) return Error.Validation("Selected fee head is not optional.");

        // PeriodLabel: stable OPT-{Code}  one live invoice per student/AY/head
        var periodLabel = PeriodLabelBuilder.ForOptIn(feeHead.Code);

        // Duplicate check
        var exists = await _db.Set<InvoiceEntity>().AnyAsync(i =>
            i.TenantId == tenantId && i.BranchId == branchId
            && i.StudentId == request.Request.StudentId
            && i.AcademicYearId == request.Request.AcademicYearId
            && i.PeriodLabel == periodLabel
            && i.Status != InvoiceStatus.Cancelled
            && i.DeletedAt == null, ct);
        if (exists)
            return Error.Conflict($"Student has already opted into {feeHead.Name} for this academic year.");

        // Student enrollment (for snapshot ClassId/SectionId)
        var enrollment = await _db.Set<StudentEnrollmentEntity>().AsNoTracking()
            .Where(e => e.StudentId == request.Request.StudentId && e.AcademicYearId == request.Request.AcademicYearId)
            .OrderByDescending(e => e.Id)
            .Select(e => new { e.ClassId, e.SectionId })
            .FirstOrDefaultAsync(ct);
        if (enrollment is null)
            return Error.Validation($"Student {request.Request.StudentId} has no enrollment for the academic year.");

        var generationDate = DateTime.UtcNow;
        var dueDate = request.Request.DueDate ?? generationDate.AddDays(15);
        var amount = request.Request.Amount;

        var invoiceNumber = await _sequence.NextInvoiceNumberAsync(branchId, ay.Code, ct);

        var invoice = new InvoiceEntity
        {
            TenantId = tenantId,
            BranchId = branchId,
            StudentId = request.Request.StudentId,
            AcademicYearId = request.Request.AcademicYearId,
            ClassId = enrollment.ClassId,
            SectionId = enrollment.SectionId,
            FeeStructureId = null,     // opt-in  not from a structure
            InvoiceNumber = invoiceNumber,
            PeriodLabel = periodLabel,
            GenerationDate = generationDate,
            DueDate = dueDate,
            TotalAmount = amount,
            ConcessionAmount = 0m,
            LateFeeAmount = 0m,
            NetPayable = amount,
            PaidAmount = 0m,
            CreditAppliedAmount = 0m,
            Status = InvoiceStatus.Draft,
            CreatedAt = DateTime.UtcNow,
        };

        invoice.Lines.Add(new InvoiceLineEntity
        {
            FeeHeadId = feeHead.Id,
            FeeHeadNameSnapshot = feeHead.Name,
            Amount = amount,
            ConcessionApplied = 0m,
            NetAmount = amount,
            IsLateFeeLine = false,
            LateFeeApplicable = false,
            LateFeeCapBase = amount,
            CreatedAt = DateTime.UtcNow,
        });

        _db.Set<InvoiceEntity>().Add(invoice);
        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("Invoices", invoice.Id, FinancialAuditAction.Create, null, invoice, ct);

        return Result.Success(InvoiceResponseFactory.ToResponse(invoice));
    }
}
