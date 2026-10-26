using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.FeeStructure;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Fees.ValueObjects;
using SchoolERP.Domain.Shared.Results;
using FeeStructureEntity = SchoolERP.Domain.Fees.Entities.FeeStructure;
using FeeStructureLineEntity = SchoolERP.Domain.Fees.Entities.FeeStructureLine;

namespace SchoolERP.Application.Features.Fees.Commands.FeeStructure.CloneFeeStructure;

public sealed class CloneFeeStructureCommandHandler : IRequestHandler<CloneFeeStructureCommand, Result<FeeStructureResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public CloneFeeStructureCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<FeeStructureResponse>> Handle(CloneFeeStructureCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var branchBelongs = await _db.Set<SchoolERP.Domain.Tenants.Entities.Branch>()
            .AnyAsync(b => b.Id == branchId && b.TenantId == tenantId, ct);
        if (!branchBelongs) return Error.Unauthorized("Branch does not belong to the current tenant.");

        var source = await _db.Set<FeeStructureEntity>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.SourceId && x.BranchId == branchId && x.DeletedAt == null, ct);
        if (source is null) return Error.NotFound("FeeStructure", request.SourceId.ToString());

        // Target AY must not already have an active/draft structure for the same scope.
        var clash = await _db.Set<FeeStructureEntity>().AnyAsync(x =>
            x.BranchId == branchId && x.AcademicYearId == request.Request.TargetAcademicYearId &&
            x.ClassGroupId == source.ClassGroupId && x.ClassId == source.ClassId &&
            x.SectionId == source.SectionId &&
            x.DeletedAt == null && x.Status != FeeStructureStatus.Archived, ct);
        if (clash) return Error.Conflict("Target academic year already has a structure for the same class/section scope.");

        var now = DateTime.UtcNow;
        var clone = new FeeStructureEntity
        {
            BranchId = branchId,
            AcademicYearId = request.Request.TargetAcademicYearId,
            ClassGroupId = source.ClassGroupId,
            ClassId = source.ClassId,
            SectionId = source.SectionId,
            Name = string.IsNullOrWhiteSpace(request.Request.NewName) ? source.Name : request.Request.NewName!.Trim(),
            ApplicableFrom = source.ApplicableFrom,
            ApplicableTo = source.ApplicableTo,
            ProrationMethod = source.ProrationMethod,
            PaymentAllocationPriority = source.PaymentAllocationPriority,
            Status = FeeStructureStatus.Draft,
            CreatedAt = now,
            Lines = source.Lines.Where(l => l.DeletedAt == null).Select(l => new FeeStructureLineEntity
            {
                FeeHeadId = l.FeeHeadId,
                Amount = AdjustAmount(l.Amount, request.Request.PercentageAdjustment, request.Request.FlatAdjustment),
                Frequency = l.Frequency,
                DueDay = l.DueDay,
                LateFeeApplicable = l.LateFeeApplicable,
                LateFeeType = l.LateFeeType,
                LateFeeValue = l.LateFeeValue,
                LateFeeGraceDays = l.LateFeeGraceDays,
                LateFeeCapPercent = l.LateFeeCapPercent,
                LateFeeSlabConfig = l.LateFeeSlabConfig,
                CreatedAt = now,
            }).ToList(),
        };
        _db.Set<FeeStructureEntity>().Add(clone);
        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("FeeStructures", clone.Id, FinancialAuditAction.Create, source.Id, clone, ct);

        return Result.Success(FeeStructureResponseFactory.ToResponse(clone));
    }

    private static decimal AdjustAmount(decimal amount, decimal? pct, decimal? flat)
    {
        if (pct.HasValue) return Money.Round(amount * (1 + pct.Value / 100m));
        if (flat.HasValue) return Money.Round(amount + flat.Value);
        return amount;
    }
}
