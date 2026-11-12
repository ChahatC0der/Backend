using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Fees.DTOs.Concession;
using SchoolERP.Domain.Shared.Results;
using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;

namespace SchoolERP.Application.Features.Fees.Queries.Concession.ExportConcessions;

public sealed class ExportConcessionsQueryHandler
    : IRequestHandler<ExportConcessionsQuery, Result<byte[]>>
{
    private const int MaxExportRows = 5000;

    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;
    private readonly IExcelExportService _excel;

    public ExportConcessionsQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant,
                                         ICurrentBranchService branch, IExcelExportService excel)
    { _db = db; _tenant = tenant; _branch = branch; _excel = excel; }

    public async Task<Result<byte[]>> Handle(ExportConcessionsQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var query = _db.Set<ConcessionEntity>().AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.BranchId == branchId && x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var categoryValue = request.Category!.Trim();
            var matched = EnumMaps_JsonAdapter.ConcessionCategoryMap.FirstOrDefault(kv =>
                string.Equals(kv.Value, categoryValue, StringComparison.OrdinalIgnoreCase));
            if (matched.Value is null)
                return Error.Validation($"Invalid category '{request.Category}'.");
            var categoryKey = matched.Key;
            query = query.Where(x => x.Category == categoryKey);
        }

        if (request.IsAutoApply.HasValue)
            query = query.Where(x => x.IsAutoApply == request.IsAutoApply.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(x => x.Name.Contains(term));
        }

        var rows = await query.OrderBy(x => x.Id).Take(MaxExportRows).ToListAsync(ct);

        var headIds = rows.Where(r => r.FeeHeadId.HasValue).Select(r => r.FeeHeadId!.Value).Distinct().ToList();
        var lookup = await _db.Set<FeeHeadEntity>().AsNoTracking()
            .Where(h => headIds.Contains(h.Id))
            .ToDictionaryAsync(h => h.Id, h => h.Name, ct);

        var data = rows.Select(c => new ConcessionExportRow
        {
            Id = c.Id.ToString(),
            Name = c.Name,
            Category = EnumMaps_JsonAdapter.ConcessionCategoryMap[c.Category],
            CalculationType = EnumMaps_JsonAdapter.ConcessionCalculationTypeMap[c.CalculationType],
            Value = c.Value,
            FeeHead = c.FeeHeadId.HasValue && lookup.TryGetValue(c.FeeHeadId.Value, out var headName) ? headName : null,
            IsAutoApply = c.IsAutoApply ? "Yes" : "No",
            ApprovalRequired = c.ApprovalRequired ? "Yes" : "No",
            StartDate = c.StartDate,
            EndDate = c.EndDate,
        }).ToList();

        var bytes = _excel.Export(data, "Concessions");
        return Result.Success(bytes);
    }
}