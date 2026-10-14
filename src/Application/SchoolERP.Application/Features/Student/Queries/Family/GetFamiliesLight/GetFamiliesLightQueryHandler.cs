using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using FamilyEntity = SchoolERP.Domain.Student.Entities.Family;

namespace SchoolERP.Application.Features.Student.Queries.Family.GetFamiliesLight;

public class GetFamiliesLightQueryHandler : IRequestHandler<GetFamiliesLightQuery, Result<List<FamilyLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetFamiliesLightQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<List<FamilyLightResponse>>> Handle(GetFamiliesLightQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var items = await _dbContext.Set<FamilyEntity>()
            .AsNoTracking()
            .Include(f => f.Students)
            .Where(f => f.BranchId == branchId && !f.IsDeleted)
            .OrderBy(f => f.HeadName)
            .ProjectToType<FamilyLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}
