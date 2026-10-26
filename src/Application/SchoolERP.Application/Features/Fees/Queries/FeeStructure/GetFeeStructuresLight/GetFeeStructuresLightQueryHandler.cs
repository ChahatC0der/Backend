using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.FeeStructure;
using SchoolERP.Domain.Fees.Enums;
using SchoolERP.Domain.Shared.Results;
using FeeStructureEntity = SchoolERP.Domain.Fees.Entities.FeeStructure;

namespace SchoolERP.Application.Features.Fees.Queries.FeeStructure.GetFeeStructuresLight;

public sealed class GetFeeStructuresLightQueryHandler : IRequestHandler<GetFeeStructuresLightQuery, Result<IReadOnlyList<FeeStructureLightResponse>>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetFeeStructuresLightQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<IReadOnlyList<FeeStructureLightResponse>>> Handle(GetFeeStructuresLightQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var q = _db.Set<FeeStructureEntity>().AsNoTracking()
            .Where(x => x.BranchId == branchId && x.DeletedAt == null && x.Status == FeeStructureStatus.Active);
        if (request.AcademicYearId.HasValue) q = q.Where(x => x.AcademicYearId == request.AcademicYearId.Value);

        var rows = await q.OrderBy(x => x.Name)
            .Select(x => new FeeStructureLightResponse
            {
                Id = x.Id, Name = x.Name, AcademicYearId = x.AcademicYearId, Status = x.Status.ToString(),
            }).ToListAsync(ct);

        return Result.Success<IReadOnlyList<FeeStructureLightResponse>>(rows);
    }
}
