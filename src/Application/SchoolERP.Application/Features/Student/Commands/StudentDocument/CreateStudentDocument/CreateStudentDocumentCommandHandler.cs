using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using StudentDocumentEntity = SchoolERP.Domain.Student.Entities.StudentDocument;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Student.Commands.StudentDocument.CreateStudentDocument;

public class CreateStudentDocumentCommandHandler : IRequestHandler<CreateStudentDocumentCommand, Result<StudentDocumentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;
    private readonly ICurrentUserService _currentUserService;

    public CreateStudentDocumentCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentBranchService branchService,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<StudentDocumentResponse>> Handle(CreateStudentDocumentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var student = await _dbContext.Set<StudentEntity>()
            .FirstOrDefaultAsync(s => s.Id == request.StudentId && s.BranchId == branchId && !s.IsDeleted, cancellationToken);
        if (student == null) return Error.NotFound("Student", request.StudentId.ToString());

        var document = request.Adapt<StudentDocumentEntity>();
        document.Verified = false;
        document.UploadedBy = _currentUserService.GetUserId();

        _dbContext.Set<StudentDocumentEntity>().Add(document);

        return Result.Success(document.Adapt<StudentDocumentResponse>());
    }
}