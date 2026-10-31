
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.FeeStructure;
using SchoolERP.Domain.Shared.Results;

using FeeStructureEntity = SchoolERP.Domain.Fees.Entities.FeeStructure;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;

namespace SchoolERP.Application.Features.Fees.Queries.FeeStructure.GetFeeStructures;

public sealed class GetFeeStructuresQueryHandler
    : IRequestHandler<GetFeeStructuresQuery, Result<PagedResponse<FeeStructureResponse>>>
{
    private static readonly HashSet<string> AllowedSort = new(StringComparer.OrdinalIgnoreCase)
    {
        "Name",
        "Status",
        "CreatedAt",
        "AcademicYearId"
    };

    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetFeeStructuresQueryHandler(
        IApplicationDbContext db,
        ICurrentTenantService tenant,
        ICurrentBranchService branch)
    {
        _db = db;
        _tenant = tenant;
        _branch = branch;
    }

    public async Task<Result<PagedResponse<FeeStructureResponse>>> Handle(
        GetFeeStructuresQuery request,
        CancellationToken ct)
    {
        // 1. Validate current tenant and branch scope.
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);

        if (scope.IsFailure)
            return scope.Error!;

        var (_, branchId) = scope.Value;

        // 2. Normalize pagination.
        var page = request.Page <= 0 ? 1 : request.Page;
        var size = request.Size <= 0 ? 10 : Math.Min(request.Size, 200);

        // 3. Build FeeStructure query.
        var query = _db.Set<FeeStructureEntity>()
            .AsNoTracking()
            .Include(x => x.Lines)
            .Where(x =>
                x.BranchId == branchId &&
                x.DeletedAt == null);

        // 4. Apply optional filters.
        if (request.AcademicYearId.HasValue)
        {
            query = query.Where(x =>
                x.AcademicYearId == request.AcademicYearId.Value);
        }

        if (request.ClassGroupId.HasValue)
        {
            query = query.Where(x =>
                x.ClassGroupId == request.ClassGroupId.Value);
        }

        if (request.ClassId.HasValue)
        {
            query = query.Where(x =>
                x.ClassId == request.ClassId.Value);
        }

        if (request.SectionId.HasValue)
        {
            query = query.Where(x =>
                x.SectionId == request.SectionId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(x =>
                x.Status == request.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();

            query = query.Where(x =>
                x.Name.Contains(term));
        }

        // 5. Apply sorting.
        var sortBy = AllowedSort.Contains(request.SortBy ?? "")
            ? request.SortBy!
            : "Name";

        var desc = string.Equals(
            request.SortOrder,
            "desc",
            StringComparison.OrdinalIgnoreCase);

        query = sortBy switch
        {
            "Status" => desc
                ? query.OrderByDescending(x => x.Status)
                : query.OrderBy(x => x.Status),

            "CreatedAt" => desc
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt),

            "AcademicYearId" => desc
                ? query.OrderByDescending(x => x.AcademicYearId)
                : query.OrderBy(x => x.AcademicYearId),

            _ => desc
                ? query.OrderByDescending(x => x.Name)
                : query.OrderBy(x => x.Name)
        };

        // 6. Get total count before pagination.
        var total = await query.CountAsync(ct);

        // 7. Fetch current page with FeeStructureLines.
        var rows = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        // 8. Collect distinct FeeHead IDs from the current page.
        var feeHeadIds = rows
            .SelectMany(x => x.Lines)
            .Select(x => x.FeeHeadId)
            .Distinct()
            .ToArray();

        // 9. Fetch all required FeeHeads in a single query.
        // Inactive FeeHeads are also included because historical
        // FeeStructureLines may reference them.
        var feeHeadLookup = feeHeadIds.Length == 0
            ? new Dictionary<long, (string Name, string Code)>()
            : (await _db.Set<FeeHeadEntity>()
                .AsNoTracking()
                .Where(x =>
                    x.BranchId == branchId &&
                    x.DeletedAt == null &&
                    feeHeadIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Code
                })
                .ToListAsync(ct))
                .ToDictionary(
                    x => x.Id,
                    x => (Name: x.Name, Code: x.Code));

        // 10. Map entities to response using FeeHead lookup.
        var responseData = rows
            .Select(x =>
                FeeStructureResponseFactory.ToResponse(x, feeHeadLookup))
            .ToList();

        // 11. Return paginated response.
        return Result.Success(new PagedResponse<FeeStructureResponse>
        {
            Data = responseData,
            Page = page,
            Size = size,
            TotalCount = total
        });
    }
}
