using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using ParentEntity = SchoolERP.Domain.Student.Entities.Parent;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Student.Queries.Parent.GetParentsLight;

public class GetParentsLightQueryHandler : IRequestHandler<GetParentsLightQuery, Result<List<ParentLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetParentsLightQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<List<ParentLightResponse>>> Handle(GetParentsLightQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var studentExists = await _dbContext.Set<StudentEntity>()
            .AnyAsync(s => s.Id == query.StudentId && s.BranchId == branchId && !s.IsDeleted, cancellationToken);
        if (!studentExists)
            return Error.NotFound("Student", query.StudentId.ToString());

        var items = await _dbContext.Set<ParentEntity>()
            .AsNoTracking()
            .Where(p => p.StudentId == query.StudentId && !p.IsDeleted)
            .OrderByDescending(p => p.IsPrimary)
            .ThenBy(p => p.ParentType)
            .ProjectToType<ParentLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}
