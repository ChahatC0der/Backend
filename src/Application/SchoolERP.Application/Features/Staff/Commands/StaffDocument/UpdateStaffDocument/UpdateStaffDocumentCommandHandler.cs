using Mapster;
using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;
using StaffDocumentEntity = SchoolERP.Domain.Staff.Entities.StaffDocument;

namespace SchoolERP.Application.Features.Staff.Commands.StaffDocument.UpdateStaffDocument;

public class UpdateStaffDocumentCommandHandler : IRequestHandler<UpdateStaffDocumentCommand, Result<StaffDocumentResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateStaffDocumentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<StaffDocumentResponse>> Handle(UpdateStaffDocumentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var entityResult = await _dbContext.GetEntityAsync<StaffDocumentEntity>(
            d => d.Id == request.Id && !d.IsDeleted,
            "StaffDocument", request.Id.ToString(), cancellationToken);
        if (entityResult.IsFailure) return entityResult.Error;

        var document = entityResult.Value;

        var staffExists = await _dbContext.EnsureEntityExistsAsync<StaffEntity>(request.StaffId, cancellationToken);
        if (staffExists != null) return Error.NotFound("Staff", request.StaffId.ToString());

        document.StaffId = request.StaffId;
        document.DocumentType = request.DocumentType;
        document.FileUrl = request.FileUrl;
        document.ExpiryDate = request.ExpiryDate;
        document.VerificationStatus = request.VerificationStatus;
        document.VerifiedBy = request.VerifiedBy;
        document.VerifiedAt = request.VerificationStatus == "verified" ? DateTime.UtcNow : null;
        document.UpdatedAt = DateTime.UtcNow;

        return Result.Success(document.Adapt<StaffDocumentResponse>());
    }
}
