using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.FeeStructure;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using FeeStructureEntity = SchoolERP.Domain.Fees.Entities.FeeStructure;
using FeeStructureLineEntity = SchoolERP.Domain.Fees.Entities.FeeStructureLine;

namespace SchoolERP.Application.Features.Fees.Commands.FeeStructure.CreateFeeStructure;

public sealed class CreateFeeStructureCommandHandler : IRequestHandler<CreateFeeStructureCommand, Result<FeeStructureResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public CreateFeeStructureCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<FeeStructureResponse>> Handle(CreateFeeStructureCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var branchBelongs = await _db.Set<SchoolERP.Domain.Tenants.Entities.Branch>()
            .AnyAsync(b => b.Id == branchId && b.TenantId == tenantId, ct);
        if (!branchBelongs) return Error.Unauthorized("Branch does not belong to the current tenant.");

        if (!TryParseEnum<ProrationMethod>(EnumMaps_JsonAdapter.ProrationMethodMap, request.Request.ProrationMethod, out var proration))
            return Error.Validation($"Invalid ProrationMethod '{request.Request.ProrationMethod}'.");
        if (!TryParseEnum<PaymentAllocationPriority>(EnumMaps_JsonAdapter.PaymentAllocationPriorityMap, request.Request.PaymentAllocationPriority, out var priority))
            return Error.Validation($"Invalid PaymentAllocationPriority '{request.Request.PaymentAllocationPriority}'.");

        // ── Parse LateFeeMode (parent-level) ──
        if (!TryParseEnum<LateFeeMode>(EnumMaps_JsonAdapter.LateFeeModeMap, request.Request.LateFeeMode, out var lateFeeMode))
            return Error.Validation($"Invalid LateFeeMode '{request.Request.LateFeeMode}'. Must be one of: none, per_head, whole_invoice.");

        // Validate each line
        var newLines = new List<FeeStructureLineEntity>();
        foreach (var line in request.Request.Lines)
        {
            if (!TryParseEnum<FeeFrequency>(EnumMaps_JsonAdapter.FeeFrequencyMap, line.Frequency, out var freq))
                return Error.Validation($"Invalid Frequency '{line.Frequency}' on fee head {line.FeeHeadId}.");

            LateFeeType? lateType = null;
            if (!string.IsNullOrWhiteSpace(line.LateFeeType))
            {
                if (!TryParseEnum<LateFeeType>(EnumMaps_JsonAdapter.LateFeeTypeMap, line.LateFeeType!, out var lt))
                    return Error.Validation($"Invalid LateFeeType '{line.LateFeeType}'.");
                lateType = lt;
            }

            if (lateType == LateFeeType.Graduated && string.IsNullOrWhiteSpace(line.LateFeeSlabConfig))
                return Error.Validation("LateFeeSlabConfig is required when LateFeeType = 'graduated'.");

            newLines.Add(new FeeStructureLineEntity
            {
                FeeHeadId = line.FeeHeadId,
                Amount = line.Amount,
                Frequency = freq,
                DueDay = line.DueDay,
                LateFeeApplicable = line.LateFeeApplicable,
                LateFeeType = lateType,
                LateFeeValue = line.LateFeeValue,
                LateFeeGraceDays = line.LateFeeGraceDays,
                LateFeeCapPercent = line.LateFeeCapPercent,
                LateFeeSlabConfig = line.LateFeeSlabConfig,
                CreatedAt = DateTime.UtcNow,
            });
        }

        var entity = new FeeStructureEntity
        {
            BranchId = branchId,
            AcademicYearId = request.Request.AcademicYearId,
            ClassGroupId = request.Request.ClassGroupId,
            ClassId = request.Request.ClassId,
            SectionId = request.Request.SectionId,
            Name = request.Request.Name.Trim(),
            ApplicableFrom = request.Request.ApplicableFrom,
            ApplicableTo = request.Request.ApplicableTo,
            ProrationMethod = proration,
            PaymentAllocationPriority = priority,

            // ── Late fee (parent-level) — defensive null-forcing per mode ──
            // Even if validator is bypassed, this guard prevents stale invoice-level fields leaking into DB
            LateFeeMode = lateFeeMode,
            InvoiceLateFeeType = lateFeeMode == LateFeeMode.WholeInvoice ? request.Request.InvoiceLateFeeType : null,
            InvoiceLateFeeValue = lateFeeMode == LateFeeMode.WholeInvoice ? request.Request.InvoiceLateFeeValue : null,
            InvoiceLateFeeGraceDays = lateFeeMode == LateFeeMode.WholeInvoice ? request.Request.InvoiceLateFeeGraceDays : null,
            InvoiceLateFeeCapPercent = lateFeeMode == LateFeeMode.WholeInvoice ? request.Request.InvoiceLateFeeCapPercent : null,
            InvoiceLateFeeSlabConfig = lateFeeMode == LateFeeMode.WholeInvoice ? request.Request.InvoiceLateFeeSlabConfig : null,

            Status = FeeStructureStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            Lines = newLines,
        };
        _db.Set<FeeStructureEntity>().Add(entity);
        await _db.SaveChangesAsync(ct);

        var feeHeadIds = entity.Lines
            .Where(line => line.DeletedAt == null)
            .Select(line => line.FeeHeadId)
            .Distinct()
            .ToArray();

        var feeHeadLookup = await FeeHeadLookupBuilder.BuildAsync(
            _db,
            branchId,
            feeHeadIds,
            ct);

        return Result.Success(FeeStructureResponseFactory.ToResponse(entity, feeHeadLookup));
    }

    private static bool TryParseEnum<TEnum>(IReadOnlyDictionary<TEnum, string> map, string value, out TEnum parsed)
        where TEnum : struct, Enum
    {
        parsed = default;
        foreach (var kv in map) if (string.Equals(kv.Value, value, StringComparison.OrdinalIgnoreCase)) { parsed = kv.Key; return true; }
        return false;
    }
}