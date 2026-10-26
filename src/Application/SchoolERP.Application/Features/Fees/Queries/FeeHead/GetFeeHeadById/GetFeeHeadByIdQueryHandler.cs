using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Fees.DTOs.FeeHead;
using FeeHeadEntity = SchoolERP.Domain.Fees.Entities.FeeHead;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.FeeHead.GetFeeHeadById;

public sealed class GetFeeHeadByIdQueryHandler : IRequestHandler<GetFeeHeadByIdQuery, Result<FeeHeadResponse>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentBranchService _branch;

    public GetFeeHeadByIdQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        _db = db; _tenant = tenant; _branch = branch;
    }

    public async Task<Result<FeeHeadResponse>> Handle(GetFeeHeadByIdQuery request, CancellationToken ct)
    {
        var scope = FeeScopeGuard.RequireBranch(_tenant, _branch);
        if (scope.IsFailure) return scope.Error!;
        var (tenantId, branchId) = scope.Value;

        var e = await _db.Set<FeeHeadEntity>().AsNoTracking().FirstOrDefaultAsync(x =>
            x.Id == request.Id && x.TenantId == tenantId &&
            x.BranchId == branchId && x.DeletedAt == null, ct);
        if (e is null) return Error.NotFound("FeeHead", request.Id.ToString());

        return Result.Success(FeeHeadResponse.FromEntity(e));
    }
}
