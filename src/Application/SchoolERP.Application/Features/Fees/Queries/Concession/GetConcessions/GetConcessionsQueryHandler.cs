using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Fees.DTOs.Concession;
using SchoolERP.Domain.Shared.Results;
using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;

namespace SchoolERP.Application.Features.Fees.Queries.Concession.GetConcessions;

public sealed class GetConcessionsQueryHandler : IRequestHandler<GetConcessionsQuery, Result<PagedResponse<ConcessionResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetConcessionsQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<PagedResponse<ConcessionResponse>>> Handle(GetConcessionsQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var page = request.Page <= 0 ? 1 : request.Page;
        var size = request.Size <= 0 ? 10 : Math.Min(request.Size, 200);

        var query = _db.Set<ConcessionEntity>().AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.BranchId == branchId && x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            var typeValue = request.Type!.Trim();
            var matched = EnumMaps_JsonAdapter.ConcessionTypeMap.FirstOrDefault(kv =>
                string.Equals(kv.Value, typeValue, StringComparison.OrdinalIgnoreCase));
            if (matched.Value is not null) query = query.Where(x => x.Type == matched.Key);
        }

        if (request.IsAutoApply.HasValue) query = query.Where(x => x.IsAutoApply == request.IsAutoApply.Value);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(x => x.Name.Contains(term));
        }

        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * size).Take(size).ToListAsync(ct);

        var headIds = rows.Where(r => r.FeeHeadId.HasValue).Select(r => r.FeeHeadId!.Value).Distinct().ToList();
        var lookup = await _db.Set<FeeHeadEntity>().AsNoTracking()
            .Where(h => headIds.Contains(h.Id))
            .ToDictionaryAsync(h => h.Id, h => h.Name, ct);

        return Result.Success(new PagedResponse<ConcessionResponse>
        {
            Data = rows.Select(r => ConcessionResponse.FromEntity(r,
                r.FeeHeadId.HasValue && lookup.TryGetValue(r.FeeHeadId.Value, out var n) ? n : null)).ToList(),
            Page = page, Size = size, TotalCount = total,
            //TotalPages = (int)Math.Ceiling(total / (double)size),
        });
    }
}
