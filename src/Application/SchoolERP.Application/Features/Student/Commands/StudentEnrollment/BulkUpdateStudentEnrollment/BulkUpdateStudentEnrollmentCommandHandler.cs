using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Shared.Results;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;
using AcademicYearEntity = SchoolERP.Domain.Academic.Entities.AcademicYear;
using ClassEntity = SchoolERP.Domain.Academic.Entities.Class;
using SectionEntity = SchoolERP.Domain.Academic.Entities.Section;

namespace SchoolERP.Application.Features.Student.Commands.StudentEnrollment.BulkUpdateStudentEnrollment;

public class BulkUpdateStudentEnrollmentCommandHandler : IRequestHandler<BulkUpdateStudentEnrollmentCommand, Result<bool>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public BulkUpdateStudentEnrollmentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<bool>> Handle(BulkUpdateStudentEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var ay = await _dbContext.Set<AcademicYearEntity>()
            .FirstOrDefaultAsync(a => a.Id == request.AcademicYearId && a.BranchId == branchId && !a.IsDeleted, cancellationToken);
        if (ay == null) return Error.NotFound("AcademicYear", request.AcademicYearId.ToString());

        var cls = await _dbContext.Set<ClassEntity>()
            .FirstOrDefaultAsync(c => c.Id == request.ClassId && c.BranchId == branchId && !c.IsDeleted, cancellationToken);
        if (cls == null) return Error.NotFound("Class", request.ClassId.ToString());

        var sec = await _dbContext.Set<SectionEntity>()
            .FirstOrDefaultAsync(s => s.Id == request.SectionId && s.ClassId == request.ClassId && !s.IsDeleted, cancellationToken);
        if (sec == null) return Error.NotFound("Section", request.SectionId.ToString());

        var entities = await _dbContext.Set<StudentEnrollmentEntity>()
            .Where(e => request.Ids.Contains(e.Id) && !e.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count != request.Ids.Count)
            return Error.NotFound("StudentEnrollment", string.Join(",", request.Ids));

        foreach (var e in entities)
        {
            e.AcademicYearId = request.AcademicYearId;
            e.ClassId = request.ClassId;
            e.SectionId = request.SectionId;
            e.RollNumber = request.RollNumber;
            e.UpdatedAt = DateTime.UtcNow;
        }

        return Result.Success(true);
    }
}
