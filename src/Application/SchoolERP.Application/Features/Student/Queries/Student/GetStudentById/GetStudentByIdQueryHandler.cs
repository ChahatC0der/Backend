using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Student.Queries.Student.GetStudentById;

public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, Result<StudentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetStudentByIdQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<StudentResponse>> Handle(GetStudentByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var student = await _dbContext.Set<StudentEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.Id && s.BranchId == branchId && !s.IsDeleted, cancellationToken);

        if (student == null)
            return Error.NotFound("Student", query.Id.ToString());

        return Result.Success(student.Adapt<StudentResponse>());
    }
}
