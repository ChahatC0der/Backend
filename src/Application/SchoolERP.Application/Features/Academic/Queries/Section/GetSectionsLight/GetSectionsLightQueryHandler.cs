using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;

namespace SchoolERP.Application.Features.Academic.Queries.Section.GetSectionsLight;

public class GetSectionsLightQueryHandler : IRequestHandler<GetSectionsLightQuery, Result<List<SectionLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetSectionsLightQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<List<SectionLightResponse>>> Handle(GetSectionsLightQuery query, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();

        var items = await _dbContext.Set<SectionEntity>()
            .AsNoTracking()
            .Where(s => s.BranchId == branchId && !s.IsDeleted)
            .OrderBy(s => s.ClassId).ThenBy(s => s.Name)
            .ProjectToType<SectionLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}