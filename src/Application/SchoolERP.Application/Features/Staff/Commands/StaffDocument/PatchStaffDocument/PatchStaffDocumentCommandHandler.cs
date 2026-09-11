using Mapster;
using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using StaffDocumentEntity = SchoolERP.Domain.Staff.Entities.StaffDocument;

namespace SchoolERP.Application.Features.Staff.Commands.StaffDocument.PatchStaffDocument;

public class PatchStaffDocumentCommandHandler : IRequestHandler<PatchStaffDocumentCommand, Result<StaffDocumentResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public PatchStaffDocumentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<StaffDocumentResponse>> Handle(PatchStaffDocumentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var entityResult = await _dbContext.GetEntityAsync<StaffDocumentEntity>(
            d => d.Id == request.Id && !d.IsDeleted,
            "StaffDocument", request.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var document = entityResult.Value;

        request.DocumentType.PatchIfProvided(v => document.DocumentType = v);
        request.FileUrl.PatchIfProvided(v => document.FileUrl = v);

        if (request.ExpiryDate.HasValue) document.ExpiryDate = request.ExpiryDate.Value;

        if (!string.IsNullOrWhiteSpace(request.VerificationStatus))
        {
            document.VerificationStatus = request.VerificationStatus;
            document.VerifiedAt = request.VerificationStatus == "verified" ? DateTime.UtcNow : null;
        }

        if (request.VerifiedBy.HasValue)
            document.VerifiedBy = request.VerifiedBy;

        document.UpdatedAt = DateTime.UtcNow;

        return Result.Success(document.Adapt<StaffDocumentResponse>());
    }
}
