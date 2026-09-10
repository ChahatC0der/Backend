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
    private readonly ICurrentBranchService _branchService;


    public GetClassGroupByIdQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService,ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
        _branchService = branchService;
    }

    public async Task<Result<ClassGroupResponse>> Handle(GetClassGroupByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();

        var classGroup = await _dbContext.Set<ClassGroupEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(cg => cg.Id == query.Id && cg.BranchId == branchId && !cg.IsDeleted, cancellationToken);

        if (classGroup == null)
            return Error.NotFound("ClassGroup", query.Id.ToString());

        return Result.Success(classGroup.Adapt<ClassGroupResponse>());
    }
}