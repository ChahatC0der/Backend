using Mapster;
using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using StudentDocumentEntity = SchoolERP.Domain.Student.Entities.StudentDocument;

namespace SchoolERP.Application.Features.Student.Commands.StudentDocument.PatchStudentDocument;

public class PatchStudentDocumentCommandHandler : IRequestHandler<PatchStudentDocumentCommand, Result<StudentDocumentResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public PatchStudentDocumentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<StudentDocumentResponse>> Handle(PatchStudentDocumentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var entityResult = await _dbContext.GetEntityAsync<StudentDocumentEntity>(
            d => d.Id == request.Id && !d.IsDeleted,
            "StudentDocument", request.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var document = entityResult.Value;

        request.DocumentType.PatchIfProvided(v => document.DocumentType = v);
        request.FileUrl.PatchIfProvided(v => document.FileUrl = v);
        request.Description.PatchIfProvided(v => document.Description = v);
        if (request.Verified.HasValue) document.Verified = request.Verified.Value;
        if (request.UploadedBy.HasValue) document.UploadedBy = request.UploadedBy;

        document.UpdatedAt = DateTime.UtcNow;

        return Result.Success(document.Adapt<StudentDocumentResponse>());
    }
}