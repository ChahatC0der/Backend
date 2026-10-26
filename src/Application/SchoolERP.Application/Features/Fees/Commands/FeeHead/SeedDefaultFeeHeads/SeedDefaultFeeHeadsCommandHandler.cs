using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Commands.FeeHead.SeedDefaultFeeHeads;

public sealed class SeedDefaultFeeHeadsCommandHandler : IRequestHandler<SeedDefaultFeeHeadsCommand, Result<SeedDefaultsResult>>
{
    // SCH-FEE-04: Tuition, Admission, Registration, Exam, Library, Transport, Late Fee
    private static readonly (string Code, string Name, bool Mandatory, bool Optional, bool Refundable)[] Defaults =
    {
        ("TUITION",      "Tuition Fee",      true,  false, false),
        ("ADMISSION",    "Admission Fee",    true,  false, false),
        ("REGISTRATION", "Registration Fee", true,  false, false),
        ("EXAM",         "Examination Fee",  true,  false, false),
        ("LIBRARY",      "Library Fee",      true,  false, false),
        ("TRANSPORT",    "Transport Fee",    false, true,  false),
        ("LATE_FEE",     "Late Fee",         false, false, false),
    };

    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public SeedDefaultFeeHeadsCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<SeedDefaultsResult>> Handle(SeedDefaultFeeHeadsCommand request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var existingCodes = await _db.Set<FeeHeadEntity>()
            .Where(x => x.TenantId == tenantId && x.BranchId == branchId && x.DeletedAt == null)
            .Select(x => x.Code).ToListAsync(ct);
        var existing = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

        var now = DateTime.UtcNow;
        var inserted = new List<string>();
        var skipped = 0;

        foreach (var d in Defaults)
        {
            if (existing.Contains(d.Code)) { skipped++; continue; }
            _db.Set<FeeHeadEntity>().Add(new FeeHeadEntity
            {
                TenantId = tenantId,
                BranchId = branchId,
                Code = d.Code,
                Name = d.Name,
                IsMandatory = d.Mandatory,
                IsOptional = d.Optional,
                IsRefundable = d.Refundable,
                Status = FeeHeadStatus.Active,
                CreatedAt = now,
            });
            inserted.Add(d.Code);
        }

        await _db.SaveChangesAsync(ct);

        // TODO (audit): await _auditWriter.LogAsync("FeeHeads", 0, FinancialAuditAction.Create, null, new { Codes = inserted }, ct);
        return Result.Success(new SeedDefaultsResult(inserted.Count, skipped, inserted));
    }
}
