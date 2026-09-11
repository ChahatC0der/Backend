using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Student.Commands.Student.RestoreStudent;

public class RestoreStudentCommandHandler : IRequestHandler<RestoreStudentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public RestoreStudentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<bool>> Handle(RestoreStudentCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var student = await _dbContext.Set<StudentEntity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == command.Id && s.BranchId == branchId && s.IsDeleted, cancellationToken);
        if (student == null)
            return Error.NotFound("Student", command.Id.ToString());

        student.IsDeleted = false;
        student.DeletedAt = null;
        student.UpdatedAt = DateTime.UtcNow;

        return Result.Success(true);
    }
}
