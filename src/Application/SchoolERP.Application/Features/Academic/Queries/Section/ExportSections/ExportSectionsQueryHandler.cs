using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;

namespace SchoolERP.Application.Features.Academic.Queries.Section.ExportSections;

public class ExportSectionsQueryHandler : IRequestHandler<ExportSectionsQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public ExportSectionsQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<byte[]>> Handle(ExportSectionsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();

        var items = await _dbContext.Set<SectionEntity>()
            .AsNoTracking()
            .Where(s => s.BranchId == branchId && !s.IsDeleted)
            .OrderBy(s => s.ClassId).ThenBy(s => s.Name)
            .ProjectToType<SectionLightResponse>()
            .ToListAsync(cancellationToken);

        var csvBytes = items.ToCsv();

        return Result.Success(csvBytes);
    }
}