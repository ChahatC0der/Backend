using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;
using RegistrationDocumentEntity = SchoolERP.Domain.Student.Registration.Entities.RegistrationDocument;

namespace SchoolERP.Application.Features.Student.Queries.RegistrationDocument.GetRegistrationDocumentsLight;

public class GetRegistrationDocumentsLightQueryHandler : IRequestHandler<GetRegistrationDocumentsLightQuery, Result<List<RegistrationDocumentLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetRegistrationDocumentsLightQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<List<RegistrationDocumentLightResponse>>> Handle(GetRegistrationDocumentsLightQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var regExists = await _dbContext.Set<RegistrationEntity>()
            .AnyAsync(r => r.Id == query.RegistrationId && r.BranchId == branchId.Value && !r.IsDeleted, cancellationToken);

        if (!regExists)
            return Error.NotFound("Registration", query.RegistrationId.ToString());

        var items = await _dbContext.Set<RegistrationDocumentEntity>()
            .AsNoTracking()
            .Where(d => d.RegistrationId == query.RegistrationId && !d.IsDeleted)
            .OrderBy(d => d.DocumentType)
            .ProjectToType<RegistrationDocumentLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}
