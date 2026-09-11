using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Student.Commands.Student.DeleteStudent;

public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public DeleteStudentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<bool>> Handle(DeleteStudentCommand command, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var entityResult = await _dbContext.GetEntityAsync<StudentEntity>(
            s => s.Id == command.Id && s.BranchId == branchId && !s.IsDeleted,
            "Student", command.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var student = entityResult.Value;
        student.IsDeleted = true;
        student.DeletedAt = DateTime.UtcNow;
        student.UpdatedAt = DateTime.UtcNow;

        return Result.Success(true);
    }
}
