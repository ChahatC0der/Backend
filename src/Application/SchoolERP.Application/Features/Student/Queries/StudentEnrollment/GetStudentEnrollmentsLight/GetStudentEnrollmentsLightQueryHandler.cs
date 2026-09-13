using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;

namespace SchoolERP.Application.Features.Student.Queries.StudentEnrollment.GetStudentEnrollmentsLight;

public class GetStudentEnrollmentsLightQueryHandler : IRequestHandler<GetStudentEnrollmentsLightQuery, Result<List<StudentEnrollmentLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetStudentEnrollmentsLightQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<List<StudentEnrollmentLightResponse>>> Handle(GetStudentEnrollmentsLightQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        // Verify student belongs to branch
        var studentExists = await _dbContext.Set<StudentEntity>()
            .AnyAsync(s => s.Id == query.StudentId && s.BranchId == branchId && !s.IsDeleted, cancellationToken);
        if (!studentExists)
            return Error.NotFound("Student", query.StudentId.ToString());

        var items = await _dbContext.Set<StudentEnrollmentEntity>()
            .AsNoTracking()
            .Where(e => e.StudentId == query.StudentId && !e.IsDeleted)
            .OrderByDescending(e => e.EnrolledAt)
            .ProjectToType<StudentEnrollmentLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}
