using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.FeeStructure;
using SchoolERP.Application.Features.Fees.Builders;
using SchoolERP.Domain.Shared.Results;
using FeeStructureEntity = SchoolERP.Domain.Fees.Entities.FeeStructure;

namespace SchoolERP.Application.Features.Fees.Queries.FeeStructure.GetFeeStructureById;

public sealed class GetFeeStructureByIdQueryHandler : IRequestHandler<GetFeeStructureByIdQuery, Result<FeeStructureResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetFeeStructureByIdQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<FeeStructureResponse>> Handle(GetFeeStructureByIdQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (_, branchId) = scope.Value;

        var e = await _db.Set<FeeStructureEntity>().AsNoTracking()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.BranchId == branchId && x.DeletedAt == null, ct);
        if (e is null) return Error.NotFound("FeeStructure", request.Id.ToString());

        var feeHeadIds = e.Lines
            .Where(line => line.DeletedAt == null)
            .Select(line => line.FeeHeadId)
            .Distinct()
            .ToArray();

        var feeHeadLookup = await FeeHeadLookupBuilder.BuildAsync(
            _db,
            branchId,
            feeHeadIds,
            ct);

        return Result.Success(FeeStructureResponseFactory.ToResponse(e, feeHeadLookup));
    }
}
