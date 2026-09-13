using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentEnrollmentEntity = SchoolERP.Domain.Student.Entities.StudentEnrollment;

namespace SchoolERP.Application.Features.Student.Queries.StudentEnrollment.ExportStudentEnrollments;

public class ExportStudentEnrollmentsQueryHandler : IRequestHandler<ExportStudentEnrollmentsQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public ExportStudentEnrollmentsQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<byte[]>> Handle(ExportStudentEnrollmentsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var queryable = from e in _dbContext.Set<StudentEnrollmentEntity>()
                        join s in _dbContext.Set<StudentEntity>() on e.StudentId equals s.Id
                        where !e.IsDeleted && !s.IsDeleted && s.BranchId == branchId
                        select e;

        if (query.StudentId.HasValue)
            queryable = queryable.Where(e => e.StudentId == query.StudentId.Value);

        var items = await queryable
            .AsNoTracking()
            .OrderByDescending(e => e.EnrolledAt)
            .ProjectToType<StudentEnrollmentLightResponse>()
            .ToListAsync(cancellationToken);

        var csvBytes = items.ToCsv();

        return Result.Success(csvBytes);
    }
}
