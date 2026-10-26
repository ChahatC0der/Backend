using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Fees.DTOs.FeeStructure;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using FeeStructureEntity = SchoolERP.Domain.Fees.Entities.FeeStructure;
using FeeStructureLineEntity = SchoolERP.Domain.Fees.Entities.FeeStructureLine;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;
using InvoiceStatusEnum = SchoolERP.Domain.Fees.Enums.InvoiceStatus;

namespace SchoolERP.Application.Features.Fees.Commands.FeeStructure.UpdateFeeStructure;

public sealed class UpdateFeeStructureCommandHandler : IRequestHandler<UpdateFeeStructureCommand, Result<FeeStructureResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public UpdateFeeStructureCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<FeeStructureResponse>> Handle(UpdateFeeStructureCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        // Branch belongs to tenant
        var branchBelongs = await _db.Set<SchoolERP.Domain.Tenants.Entities.Branch>()
            .AnyAsync(b => b.Id == branchId && b.TenantId == tenantId, ct);
        if (!branchBelongs) return Error.Unauthorized("Branch does not belong to the current tenant.");

        var entity = await _db.Set<FeeStructureEntity>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.BranchId == branchId && x.DeletedAt == null, ct);
        if (entity is null) return Error.NotFound("FeeStructure", request.Id.ToString());

        if (entity.Status == FeeStructureStatus.Archived)
            return Error.Conflict("Archived fee structures cannot be edited.");

        var referenced = await _db.Set<InvoiceEntity>()
    .AnyAsync(i => i.FeeStructureId == entity.Id
                && i.Status != InvoiceStatusEnum.Cancelled
                && i.DeletedAt == null, ct);
if (referenced) return Error.Conflict(
    "This structure is referenced by non-cancelled invoices. Clone and archive this structure instead of editing.");

        if (!TryParseEnum<ProrationMethod>(EnumMaps_JsonAdapter.ProrationMethodMap, request.Request.ProrationMethod, out var proration))
            return Error.Validation($"Invalid ProrationMethod '{request.Request.ProrationMethod}'.");
        if (!TryParseEnum<PaymentAllocationPriority>(EnumMaps_JsonAdapter.PaymentAllocationPriorityMap, request.Request.PaymentAllocationPriority, out var priority))
            return Error.Validation($"Invalid PaymentAllocationPriority '{request.Request.PaymentAllocationPriority}'.");

        // Parse incoming lines before any mutation.
        var incoming = new List<(long? Id, FeeStructureLineDto Dto, FeeFrequency Freq, LateFeeType? Type)>();
        foreach (var l in request.Request.Lines)
        {
            if (!TryParseEnum<FeeFrequency>(EnumMaps_JsonAdapter.FeeFrequencyMap, l.Frequency, out var freq))
                return Error.Validation($"Invalid Frequency '{l.Frequency}' on fee head {l.FeeHeadId}.");

            LateFeeType? lt = null;
            if (!string.IsNullOrWhiteSpace(l.LateFeeType))
            {
                if (!TryParseEnum<LateFeeType>(EnumMaps_JsonAdapter.LateFeeTypeMap, l.LateFeeType!, out var parsed))
                    return Error.Validation($"Invalid LateFeeType '{l.LateFeeType}'.");
                lt = parsed;
            }
            if (lt == LateFeeType.Graduated && string.IsNullOrWhiteSpace(l.LateFeeSlabConfig))
                return Error.Validation("LateFeeSlabConfig required when LateFeeType = 'graduated'.");

            incoming.Add((l.Id, l, freq, lt));
        }

        // --- Mutations ---
        entity.Name = request.Request.Name.Trim();
        entity.ApplicableFrom = request.Request.ApplicableFrom;
        entity.ApplicableTo = request.Request.ApplicableTo;
        entity.ProrationMethod = proration;
        entity.PaymentAllocationPriority = priority;
        entity.UpdatedAt = DateTime.UtcNow;

        var now = DateTime.UtcNow;
        var keepIds = incoming.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToHashSet();

        // Soft-delete removed lines
        foreach (var existing in entity.Lines.Where(x => x.DeletedAt == null && !keepIds.Contains(x.Id)))
        {
            existing.DeletedAt = now;
            existing.UpdatedAt = now;
        }

        // Update existing + insert new
        foreach (var item in incoming)
        {
            var line = item.Id.HasValue ? entity.Lines.FirstOrDefault(x => x.Id == item.Id!.Value) : null;
            if (line is null)
            {
                line = new FeeStructureLineEntity { FeeStructureId = entity.Id, CreatedAt = now };
                entity.Lines.Add(line);
            }
            line.FeeHeadId = item.Dto.FeeHeadId;
            line.Amount = item.Dto.Amount;
            line.Frequency = item.Freq;
            line.DueDay = item.Dto.DueDay;
            line.LateFeeApplicable = item.Dto.LateFeeApplicable;
            line.LateFeeType = item.Type;
            line.LateFeeValue = item.Dto.LateFeeValue;
            line.LateFeeGraceDays = item.Dto.LateFeeGraceDays;
            line.LateFeeCapPercent = item.Dto.LateFeeCapPercent;
            line.LateFeeSlabConfig = item.Dto.LateFeeSlabConfig;
            line.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("FeeStructures", entity.Id, FinancialAuditAction.Update, null, entity, ct);

        return Result.Success(FeeStructureResponseFactory.ToResponse(entity));
    }

    private static bool TryParseEnum<TEnum>(IReadOnlyDictionary<TEnum, string> map, string value, out TEnum parsed)
        where TEnum : struct, Enum
    {
        parsed = default;
        foreach (var kv in map) if (string.Equals(kv.Value, value, StringComparison.OrdinalIgnoreCase)) { parsed = kv.Key; return true; }
        return false;
    }
}

