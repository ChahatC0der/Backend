using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using ParentEntity = SchoolERP.Domain.Student.Entities.Parent;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Student.Queries.Parent.GetParentById;

public class GetParentByIdQueryHandler : IRequestHandler<GetParentByIdQuery, Result<ParentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetParentByIdQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<ParentResponse>> Handle(GetParentByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var parent = await (
            from p in _dbContext.Set<ParentEntity>()
            join s in _dbContext.Set<StudentEntity>() on p.StudentId equals s.Id
            where p.Id == query.Id && !p.IsDeleted && !s.IsDeleted && s.BranchId == branchId
            select p
        ).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

        if (parent == null)
            return Error.NotFound("Parent", query.Id.ToString());

        return Result.Success(parent.Adapt<ParentResponse>());
    }
}
