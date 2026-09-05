using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Academic.DTOs;
using SchoolERP.Domain.Shared.Results;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;

namespace SchoolERP.Application.Features.Academic.Queries.Section.GetSectionById;

public class GetSectionByIdQueryHandler : IRequestHandler<GetSectionByIdQuery, Result<SectionResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public GetSectionByIdQueryHandler(IApplicationDbContext dbContext, ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;
    }

    public async Task<Result<SectionResponse>> Handle(GetSectionByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _tenantService.GetBranchId();

        var section = await _dbContext.Set<SectionEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.Id && s.BranchId == branchId && !s.IsDeleted, cancellationToken);

        if (section == null)
            return Error.NotFound("Section", query.Id.ToString());

        return Result.Success(section.Adapt<SectionResponse>());
    }
}