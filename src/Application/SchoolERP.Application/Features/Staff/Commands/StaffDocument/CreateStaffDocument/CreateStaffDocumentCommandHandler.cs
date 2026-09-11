using Mapster;
using MediatR;
using SchoolERP.Application.Common.Extensions;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Staff.DTOs;
using SchoolERP.Domain.Shared.Results;
using StaffEntity = SchoolERP.Domain.Staff.Entities.Staff;
using StaffDocumentEntity = SchoolERP.Domain.Staff.Entities.StaffDocument;

namespace SchoolERP.Application.Features.Staff.Commands.StaffDocument.CreateStaffDocument;

public class CreateStaffDocumentCommandHandler : IRequestHandler<CreateStaffDocumentCommand, Result<StaffDocumentResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateStaffDocumentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<StaffDocumentResponse>> Handle(CreateStaffDocumentCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var staffExists = await _dbContext.EnsureEntityExistsAsync<StaffEntity>(request.StaffId, cancellationToken);
        if (staffExists != null) return Error.NotFound("Staff", request.StaffId.ToString());

        var document = request.Adapt<StaffDocumentEntity>();
        document.VerificationStatus = "not_verified";

        _dbContext.Set<StaffDocumentEntity>().Add(document);

        return Result.Success(document.Adapt<StaffDocumentResponse>());
    }
}
