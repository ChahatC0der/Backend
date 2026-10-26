using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.FeeStructure;
using SchoolERP.Domain.Shared.Results;
using FeeStructureEntity = SchoolERP.Domain.Fees.Entities.FeeStructure;

namespace SchoolERP.Application.Features.Fees.Queries.FeeStructure.GetFeeStructures;

public sealed class GetFeeStructuresQueryHandler : IRequestHandler<GetFeeStructuresQuery, Result<PagedResponse<FeeStructureResponse>>>
{
    private static readonly HashSet<string> AllowedSort = new(StringComparer.OrdinalIgnoreCase)
        { "Name", "Status", "CreatedAt", "AcademicYearId" };

    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetFeeStructuresQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<PagedResponse<FeeStructureResponse>>> Handle(GetFeeStructuresQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var page = request.Page <= 0 ? 1 : request.Page;
        var size = request.Size <= 0 ? 10 : Math.Min(request.Size, 200);

        var query = _db.Set<FeeStructureEntity>().AsNoTracking().Include(x => x.Lines)
            .Where(x => x.BranchId == branchId && x.DeletedAt == null);

        if (request.AcademicYearId.HasValue) query = query.Where(x => x.AcademicYearId == request.AcademicYearId.Value);
        if (request.ClassGroupId.HasValue) query = query.Where(x => x.ClassGroupId == request.ClassGroupId.Value);
        if (request.ClassId.HasValue) query = query.Where(x => x.ClassId == request.ClassId.Value);
        if (request.SectionId.HasValue) query = query.Where(x => x.SectionId == request.SectionId.Value);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(x => x.Name.Contains(term));
        }

        var sortBy = AllowedSort.Contains(request.SortBy ?? "") ? request.SortBy! : "Name";
        var desc = string.Equals(request.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = sortBy switch
        {
            "Status"         => desc ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "CreatedAt"      => desc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
            "AcademicYearId" => desc ? query.OrderByDescending(x => x.AcademicYearId) : query.OrderBy(x => x.AcademicYearId),
            _                => desc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
        };

        var total = await query.CountAsync(ct);
        var rows = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        return Result.Success(new PagedResponse<FeeStructureResponse>
        {
            Data = rows.Select(r => FeeStructureResponseFactory.ToResponse(r)).ToList(),
            Page = page,
            Size = size,
            TotalCount = total,
            //TotalPages = (int)Math.Ceiling(total / (double)size),
        });
    }
}
