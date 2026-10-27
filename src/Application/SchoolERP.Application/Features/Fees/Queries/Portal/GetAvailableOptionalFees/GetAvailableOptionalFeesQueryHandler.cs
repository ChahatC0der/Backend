using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Fees.DTOs.Portal;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;
using InvoiceEntity = SchoolERP.Domain.Fees.Entities.Invoice;
using FeeStructureLineEntity = SchoolERP.Domain.Fees.Entities.FeeStructureLine;
using   FeeStructureEntity = SchoolERP.Domain.Fees.Entities.FeeStructure;

namespace SchoolERP.Application.Features.Fees.Queries.Portal.GetAvailableOptionalFees;

public sealed class GetAvailableOptionalFeesQueryHandler : IRequestHandler<GetAvailableOptionalFeesQuery, Result<IReadOnlyList<OptionalFeeResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly ICurrentUserService _user;

    public GetAvailableOptionalFeesQueryHandler(
        IApplicationDbContext db, ICurrentTenantService tenant,
        ICurrentBranchService branch, ICurrentUserService user)
    { _db = db; _tenant = tenant; _branch = branch; _user = user; }

    public async Task<Result<IReadOnlyList<OptionalFeeResponse>>> Handle(GetAvailableOptionalFeesQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var auth = await ParentChildResolver.EnsureParentAsync(_db, _user.GetUserId() ?? 0, request.StudentId, ct);
        if (auth.IsFailure) return auth.Error!;

        // All optional fee heads for this branch
        var optionalHeads = await _db.Set<FeeHeadEntity>().AsNoTracking()
            .Where(h => h.TenantId == tenantId && h.BranchId == branchId
                     && h.DeletedAt == null && h.Status == FeeHeadStatus.Active
                     && h.IsOptional)
            .ToListAsync(ct);

        if (optionalHeads.Count == 0)
            return Result.Success<IReadOnlyList<OptionalFeeResponse>>(Array.Empty<OptionalFeeResponse>());

        // Already opted-in fee heads (one invoice per student/AY/head, PeriodLabel = "OPT-{CODE}")
        var optedInLabels = await _db.Set<InvoiceEntity>().AsNoTracking()
            .Where(i => i.TenantId == tenantId && i.BranchId == branchId
                     && i.StudentId == request.StudentId && i.AcademicYearId == request.AcademicYearId
                     && i.DeletedAt == null && i.Status != InvoiceStatus.Cancelled
                     && i.PeriodLabel.StartsWith("OPT-"))
            .Select(i => i.PeriodLabel)
            .ToListAsync(ct);
        var optedInSet = new HashSet<string>(optedInLabels, StringComparer.OrdinalIgnoreCase);

        // Suggested amounts from active fee structures
        // NOTE: FeeStructure : BranchEntity (no TenantId). Tenant safety comes from branch scope + global filters.
        var headIds = optionalHeads.Select(h => h.Id).ToList();
        var suggestedAmounts = await (from line in _db.Set<FeeStructureLineEntity>().AsNoTracking()
                                       join s in _db.Set<FeeStructureEntity>().AsNoTracking() on line.FeeStructureId equals s.Id
                                       where s.BranchId == branchId
                                             && s.DeletedAt == null && s.Status == FeeStructureStatus.Active
                                             && s.AcademicYearId == request.AcademicYearId
                                             && line.DeletedAt == null && headIds.Contains(line.FeeHeadId)
                                       select new { line.FeeHeadId, line.Amount })
            .ToListAsync(ct);

        var amountLookup = suggestedAmounts
            .GroupBy(x => x.FeeHeadId)
            .ToDictionary(g => g.Key, g => (decimal?)g.First().Amount);

        var result = optionalHeads.Select(h => new OptionalFeeResponse
        {
            FeeHeadId = h.Id,
            Code = h.Code,
            Name = h.Name,
            Description = h.Description,
            SuggestedAmount = amountLookup.TryGetValue(h.Id, out var amt) ? amt : null,
            AlreadyOptedIn = optedInSet.Contains($"OPT-{h.Code.ToUpperInvariant()}"),
        }).ToList();

        return Result.Success<IReadOnlyList<OptionalFeeResponse>>(result);
    }
}
