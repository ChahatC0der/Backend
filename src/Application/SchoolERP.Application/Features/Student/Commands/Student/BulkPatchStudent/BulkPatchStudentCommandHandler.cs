using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Student.Commands.Student.BulkPatchStudent;

public class BulkPatchStudentCommandHandler : IRequestHandler<BulkPatchStudentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public BulkPatchStudentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<bool>> Handle(BulkPatchStudentCommand command, CancellationToken cancellationToken)
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
            request.Status.PatchIfProvided(v => s.Status = v);
            request.BloodGroup.PatchIfProvided(v => s.BloodGroup = v);
            request.Nationality.PatchIfProvided(v => s.Nationality = v);
            s.UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success(true);
    }
}
