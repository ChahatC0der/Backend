using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using FamilyEntity = SchoolERP.Domain.Student.Entities.Family;

namespace SchoolERP.Application.Features.Student.Queries.Family.GetFamilyById;

public class GetFamilyByIdQueryHandler : IRequestHandler<GetFamilyByIdQuery, Result<FamilyResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetFamilyByIdQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<FamilyResponse>> Handle(GetFamilyByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var family = await _dbContext.Set<FamilyEntity>()
            .AsNoTracking()
            .Include(f => f.Students)
            .FirstOrDefaultAsync(f => f.Id == query.Id && f.BranchId == branchId && !f.IsDeleted, cancellationToken);

        if (family == null) return Error.NotFound("Family", query.Id.ToString());

        return Result.Success(family.Adapt<FamilyResponse>());
    }
}
