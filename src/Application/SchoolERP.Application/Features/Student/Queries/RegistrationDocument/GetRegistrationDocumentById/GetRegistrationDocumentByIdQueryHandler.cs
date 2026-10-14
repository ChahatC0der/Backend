using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;
using RegistrationDocumentEntity = SchoolERP.Domain.Student.Registration.Entities.RegistrationDocument;

namespace SchoolERP.Application.Features.Student.Queries.RegistrationDocument.GetRegistrationDocumentById;

public class GetRegistrationDocumentByIdQueryHandler : IRequestHandler<GetRegistrationDocumentByIdQuery, Result<RegistrationDocumentResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetRegistrationDocumentByIdQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<RegistrationDocumentResponse>> Handle(GetRegistrationDocumentByIdQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var doc = await (
            from d in _dbContext.Set<RegistrationDocumentEntity>()
            join r in _dbContext.Set<RegistrationEntity>() on d.RegistrationId equals r.Id
            where d.Id == query.Id && !d.IsDeleted && !r.IsDeleted && r.BranchId == branchId.Value
            select d
        ).AsNoTracking().FirstOrDefaultAsync(cancellationToken);

        if (doc == null)
            return Error.NotFound("RegistrationDocument", query.Id.ToString());

        return Result.Success(doc.Adapt<RegistrationDocumentResponse>());
    }
}
