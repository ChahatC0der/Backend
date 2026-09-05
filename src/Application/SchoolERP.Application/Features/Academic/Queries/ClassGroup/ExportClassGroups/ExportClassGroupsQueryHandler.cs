using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;
using ClassGroupEntity = SchoolERP.Domain.Academic.Entities.ClassGroup;

namespace SchoolERP.Application.Features.Academic.Queries.ClassGroup.ExportClassGroups;

public class ExportClassGroupsQueryHandler : IRequestHandler<ExportClassGroupsQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public ExportClassGroupsQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<byte[]>> Handle(ExportClassGroupsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();

        var items = await _dbContext.Set<ClassGroupEntity>()
            .AsNoTracking()
            .Where(cg => cg.BranchId == branchId && !cg.IsDeleted)
            .OrderBy(cg => cg.Sequence)
            .ProjectToType<ClassGroupLightResponse>()
            .ToListAsync(cancellationToken);

        var csvBytes = items.ToCsv();

        return Result.Success(csvBytes);
    }
}