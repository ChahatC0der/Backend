using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using StudentDocumentEntity = SchoolERP.Domain.Student.Entities.StudentDocument;
using StudentEntity = SchoolERP.Domain.Student.Entities.Student;

namespace SchoolERP.Application.Features.Student.Commands.StudentDocument.UpdateStudentDocument;

public class UpdateStudentDocumentCommandHandler : IRequestHandler<UpdateStudentDocumentCommand, Result<StudentDocumentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public UpdateStudentDocumentCommandHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<StudentDocumentResponse>> Handle(UpdateStudentDocumentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var branchId = _branchService.GetBranchId() ?? Guid.Empty;

        var entityResult = await _dbContext.GetEntityAsync<StudentDocumentEntity>(
            d => d.Id == request.Id && !d.IsDeleted,
            "StudentDocument", request.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var document = entityResult.Value;

        var student = await _dbContext.Set<StudentEntity>()
            .FirstOrDefaultAsync(s => s.Id == request.StudentId && s.BranchId == branchId && !s.IsDeleted, cancellationToken);
        if (student == null) return Error.NotFound("Student", request.StudentId.ToString());

        document.StudentId = request.StudentId;
        document.DocumentType = request.DocumentType;
        document.FileUrl = request.FileUrl;
        document.Description = request.Description;
        document.Verified = request.Verified;
        document.UploadedBy = request.UploadedBy;
        document.UpdatedAt = DateTime.UtcNow;

        return Result.Success(document.Adapt<StudentDocumentResponse>());
    }
}