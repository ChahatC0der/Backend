using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;

namespace SchoolERP.Application.Features.Academic.Queries.Class.ExportClasses;

public class ExportClassesQueryHandler : IRequestHandler<ExportClassesQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public ExportClassesQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<byte[]>> Handle(ExportClassesQuery query, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();

        var items = await _dbContext.Set<ClassEntity>()
            .AsNoTracking()
            .Where(c => c.BranchId == branchId && !c.IsDeleted)
            .OrderBy(c => c.Sequence)
            .ProjectToType<ClassLightResponse>()
            .ToListAsync(cancellationToken);

        var csvBytes = items.ToCsv();

        return Result.Success(csvBytes);
    }
}