using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;
using StudentDocumentEntity = SchoolERP.Domain.Student.Entities.StudentDocument;

namespace SchoolERP.Application.Features.Student.Queries.StudentDocument.GetStudentDocumentById;

public class GetStudentDocumentByIdQueryHandler : IRequestHandler<GetStudentDocumentByIdQuery, Result<StudentDocumentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetStudentDocumentByIdQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<StudentDocumentResponse>> Handle(GetStudentDocumentByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var document = await (
            from d in _dbContext.Set<StudentDocumentEntity>()
            join s in _dbContext.Set<StudentEntity>() on d.StudentId equals s.Id
            where d.Id == query.Id && !d.IsDeleted && !s.IsDeleted && s.BranchId == branchId
            select d
        ).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

        if (document == null)
            return Error.NotFound("StudentDocument", query.Id.ToString());

        return Result.Success(document.Adapt<StudentDocumentResponse>());
    }
}
