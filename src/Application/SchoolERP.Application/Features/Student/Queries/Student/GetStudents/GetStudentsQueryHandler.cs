using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Student.Queries.Student.GetStudents;

public class GetStudentsQueryHandler : IRequestHandler<GetStudentsQuery, Result<PagedResponse<StudentResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetStudentsQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<PagedResponse<StudentResponse>>> Handle(GetStudentsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;
        var request = query.Request;

        var queryable = _dbContext.Set<StudentEntity>()
            .AsNoTracking()
            .Where(s => s.BranchId == branchId && !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            queryable = queryable.Where(s =>
                s.FirstName.ToLower().Contains(search) ||
                (s.LastName != null && s.LastName.ToLower().Contains(search)) ||
                s.EnrollmentId.ToLower().Contains(search) ||
                (s.PersonalMobile != null && s.PersonalMobile.Contains(search)) ||
                (s.RollNumber != null && s.RollNumber.ToLower().Contains(search)));
        }

        queryable = request.SortBy?.ToLower() switch
        {
            "firstname" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(s => s.FirstName) : queryable.OrderBy(s => s.FirstName),
            "enrollmentid" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(s => s.EnrollmentId) : queryable.OrderBy(s => s.EnrollmentId),
            "admissiondate" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(s => s.AdmissionDate) : queryable.OrderBy(s => s.AdmissionDate),
            "status" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(s => s.Status) : queryable.OrderBy(s => s.Status),
            _ => queryable.OrderBy(s => s.FirstName)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var data = items.Select(x => x.Adapt<StudentResponse>()).ToList();

        return Result.Success(new PagedResponse<StudentResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}
