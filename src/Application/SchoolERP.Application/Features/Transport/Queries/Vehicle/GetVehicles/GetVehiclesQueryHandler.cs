using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Application.Features.Transport.Commands.Vehicle.CreateVehicle;
using SchoolERP.Application.Features.Transport.DTOs.Vehicle;
using SchoolERP.Domain.Shared.Results;
using VehicleEntity = SchoolERP.Domain.Transport.Entities.Vehicle;

namespace SchoolERP.Application.Features.Transport.Queries.Vehicle.GetVehicles;

public sealed class GetVehiclesQueryHandler : IRequestHandler<GetVehiclesQuery, Result<PagedResponse<VehicleResponse>>>
{
    private static readonly HashSet<string> AllowedSort = new(StringComparer.OrdinalIgnoreCase)
        { "VehicleNumber", "Status", "Capacity", "CreatedAt" };

    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetVehiclesQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    { _db = db; _tenant = tenant; _branch = branch; }

    public async Task<Result<PagedResponse<VehicleResponse>>> Handle(GetVehiclesQuery request, CancellationToken ct)
    {
        var scope = TransportScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var page = request.Page <= 0 ? 1 : request.Page;
        var size = request.Size <= 0 ? 10 : Math.Min(request.Size, 200);

        var query = _db.Set<VehicleEntity>().AsNoTracking()
            .Where(v => v.TenantId == tenantId && v.BranchId == branchId && !v.IsDeleted);

        if (request.Status.HasValue) query = query.Where(v => v.Status == request.Status.Value);
        if (!string.IsNullOrWhiteSpace(request.VehicleType)
            && CreateVehicleCommandHandler.TryParseVehicleType(request.VehicleType!, out var vt))
            query = query.Where(v => v.VehicleType == vt);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(v => v.VehicleNumber.Contains(term)
                                  || (v.Model != null && v.Model.Contains(term))
                                  || (v.Make != null && v.Make.Contains(term)));
        }

        var sortBy = AllowedSort.Contains(request.SortBy ?? "") ? request.SortBy! : "VehicleNumber";
        var desc = string.Equals(request.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = sortBy switch
        {
            "Status"    => desc ? query.OrderByDescending(v => v.Status)        : query.OrderBy(v => v.Status),
            "Capacity"  => desc ? query.OrderByDescending(v => v.Capacity)      : query.OrderBy(v => v.Capacity),
            "CreatedAt" => desc ? query.OrderByDescending(v => v.CreatedAt)     : query.OrderBy(v => v.CreatedAt),
            _           => desc ? query.OrderByDescending(v => v.VehicleNumber) : query.OrderBy(v => v.VehicleNumber),
        };

        var total = await query.CountAsync(ct);
        var rows = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        return Result.Success(new PagedResponse<VehicleResponse>
        {
            Data = rows.Select(VehicleResponse.FromEntity).ToList(),
            Page = page, Size = size, TotalCount = total,
            //TotalPages = (int)Math.Ceiling(total / (double)size),
        });
    }
}
