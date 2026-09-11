using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Student.Commands.Student.BulkDeleteStudent;

public class BulkDeleteStudentCommandHandler : IRequestHandler<BulkDeleteStudentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public BulkDeleteStudentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<bool>> Handle(BulkDeleteStudentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var entities = await _dbContext.Set<StudentEntity>()
            .Where(s => s.BranchId == branchId && request.Ids.Contains(s.Id) && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("Student", string.Join(",", request.Ids));

        foreach (var s in entities)
        {
            s.IsDeleted = true;
            s.DeletedAt = DateTime.UtcNow;
            s.UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success(true);
    }
}
