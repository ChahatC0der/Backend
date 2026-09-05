using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;

namespace SchoolERP.Application.Features.Academic.Queries.Class.GetClassById;

public class GetClassByIdQueryHandler : IRequestHandler<GetClassByIdQuery, Result<ClassResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetClassByIdQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<ClassResponse>> Handle(GetClassByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();

        var classEntity = await _dbContext.Set<ClassEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.Id && c.BranchId == branchId && !c.IsDeleted, cancellationToken);

        if (classEntity == null)
            return Error.NotFound("Class", query.Id.ToString());

        return Result.Success(classEntity.Adapt<ClassResponse>());
    }
}