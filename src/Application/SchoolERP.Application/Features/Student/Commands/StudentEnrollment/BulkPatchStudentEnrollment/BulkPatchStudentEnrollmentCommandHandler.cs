using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;

namespace SchoolERP.Application.Features.Student.Commands.StudentEnrollment.BulkPatchStudentEnrollment;

public class BulkPatchStudentEnrollmentCommandHandler : IRequestHandler<BulkPatchStudentEnrollmentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;

    public BulkPatchStudentEnrollmentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool>> Handle(BulkPatchStudentEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var entities = await _dbContext.Set<StudentEnrollmentEntity>()
            .Where(e => request.Ids.Contains(e.Id) && !e.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("StudentEnrollment", string.Join(",", request.Ids));

        foreach (var e in entities)
        {
            if (request.AcademicYearId.HasValue) e.AcademicYearId = request.AcademicYearId.Value;
            if (request.ClassId.HasValue) e.ClassId = request.ClassId.Value;
            if (request.SectionId.HasValue) e.SectionId = request.SectionId.Value;
            request.RollNumber.PatchIfProvided(v => e.RollNumber = v);
            if (request.IsCurrent.HasValue) e.IsCurrent = request.IsCurrent.Value;
            e.UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success(true);
    }
}
