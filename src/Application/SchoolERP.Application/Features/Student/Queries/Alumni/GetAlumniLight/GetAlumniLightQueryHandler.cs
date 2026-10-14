using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using AlumniEntity = SchoolERP.Domain.Student.Alumni.Entities.Alumni;

namespace SchoolERP.Application.Features.Student.Queries.Alumni.GetAlumniLight;

public class GetAlumniLightQueryHandler : IRequestHandler<GetAlumniLightQuery, Result<List<AlumniLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetAlumniLightQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<List<AlumniLightResponse>>> Handle(GetAlumniLightQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (branchId == null) return Error.Conflict("Please Choose a Branch");

        var items = await _dbContext.Set<AlumniEntity>()
            .AsNoTracking()
            .Where(a => a.BranchId == branchId && !a.IsDeleted && !a.IsArchived)
            .OrderBy(a => a.FirstName)
            .ProjectToType<AlumniLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}
