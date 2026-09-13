using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentDocumentEntity = SchoolERP.Domain.Student.Entities.StudentDocument;

namespace SchoolERP.Application.Features.Student.Queries.StudentDocument.GetStudentDocumentsLight;

public class GetStudentDocumentsLightQueryHandler : IRequestHandler<GetStudentDocumentsLightQuery, Result<List<StudentDocumentLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetStudentDocumentsLightQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<List<StudentDocumentLightResponse>>> Handle(GetStudentDocumentsLightQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var studentExists = await _dbContext.Set<StudentEntity>()
            .AnyAsync(s => s.Id == query.StudentId && s.BranchId == branchId && !s.IsDeleted, cancellationToken);
        if (!studentExists)
            return Error.NotFound("Student", query.StudentId.ToString());

        var items = await _dbContext.Set<StudentDocumentEntity>()
            .AsNoTracking()
            .Where(d => d.StudentId == query.StudentId && !d.IsDeleted)
            .OrderBy(d => d.DocumentType)
            .ProjectToType<StudentDocumentLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}
