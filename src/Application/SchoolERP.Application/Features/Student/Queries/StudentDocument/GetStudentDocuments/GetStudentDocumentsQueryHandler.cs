using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Common.DTOs;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentDocumentEntity = SchoolERP.Domain.Student.Entities.StudentDocument;

namespace SchoolERP.Application.Features.Student.Queries.StudentDocument.GetStudentDocuments;

public class GetStudentDocumentsQueryHandler : IRequestHandler<GetStudentDocumentsQuery, Result<PagedResponse<StudentDocumentResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetStudentDocumentsQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<PagedResponse<StudentDocumentResponse>>> Handle(GetStudentDocumentsQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;
        var request = query.Request;

        var queryable = from d in _dbContext.Set<StudentDocumentEntity>()
                        join s in _dbContext.Set<StudentEntity>() on d.StudentId equals s.Id
                        where !d.IsDeleted && !s.IsDeleted && s.BranchId == branchId
                        select d;

        if (query.StudentId.HasValue)
            queryable = queryable.Where(d => d.StudentId == query.StudentId.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.ToLower();
            queryable = queryable.Where(d => d.DocumentType.ToLower().Contains(search));
        }

        queryable = request.SortBy?.ToLower() switch
        {
            "documenttype" => request.SortOrder?.ToLower() == "desc" ? queryable.OrderByDescending(d => d.DocumentType) : queryable.OrderBy(d => d.DocumentType),
            _ => queryable.OrderBy(d => d.DocumentType)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip((request.Page - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync(cancellationToken);

        var data = items.Select(x => x.Adapt<StudentDocumentResponse>()).ToList();

        return Result.Success(new PagedResponse<StudentDocumentResponse>
        {
            Data = data,
            TotalCount = totalCount,
            Page = request.Page,
            Size = request.Size
        });
    }
}
