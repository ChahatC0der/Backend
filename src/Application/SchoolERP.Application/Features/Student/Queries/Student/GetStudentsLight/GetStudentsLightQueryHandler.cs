using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Student.Queries.Student.GetStudentsLight;

public class GetStudentsLightQueryHandler : IRequestHandler<GetStudentsLightQuery, Result<List<StudentLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetStudentsLightQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<List<StudentLightResponse>>> Handle(GetStudentsLightQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var items = await _dbContext.Set<StudentEntity>()
            .AsNoTracking()
            .Where(s => s.BranchId == branchId && !s.IsDeleted && s.Status == "active")
            .OrderBy(s => s.FirstName)
            .ProjectToType<StudentLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}
