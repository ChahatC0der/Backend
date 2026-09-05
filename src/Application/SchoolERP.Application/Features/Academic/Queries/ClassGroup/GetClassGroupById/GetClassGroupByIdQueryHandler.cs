using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;
using ClassGroupEntity = SchoolERP.Domain.Academic.Entities.ClassGroup;

namespace SchoolERP.Application.Features.Academic.Queries.ClassGroup.GetClassGroupById;

public class GetClassGroupByIdQueryHandler : IRequestHandler<GetClassGroupByIdQuery, Result<ClassGroupResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetClassGroupByIdQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<ClassGroupResponse>> Handle(GetClassGroupByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();

        var classGroup = await _dbContext.Set<ClassGroupEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(cg => cg.Id == query.Id && cg.BranchId == branchId && !cg.IsDeleted, cancellationToken);

        if (classGroup == null)
            return Error.NotFound("ClassGroup", query.Id.ToString());

        return Result.Success(classGroup.Adapt<ClassGroupResponse>());
    }
}