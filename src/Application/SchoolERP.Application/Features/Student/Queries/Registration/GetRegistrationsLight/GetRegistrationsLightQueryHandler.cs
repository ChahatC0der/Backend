using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;
using RegistrationEntity = SchoolERP.Domain.Student.Registration.Entities.Registration;

namespace SchoolERP.Application.Features.Student.Queries.Registration.GetRegistrationsLight;

public class GetRegistrationsLightQueryHandler : IRequestHandler<GetRegistrationsLightQuery, Result<List<RegistrationLightResponse>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentBranchService _branchService;

    public GetRegistrationsLightQueryHandler(IApplicationDbContext dbContext, ICurrentBranchService branchService)
    {
        _dbContext = dbContext;
        _branchService = branchService;
    }

    public async Task<Result<List<RegistrationLightResponse>>> Handle(GetRegistrationsLightQuery query, CancellationToken cancellationToken)
    {
        var branchId = _branchService.GetBranchId();
        if (!branchId.HasValue || branchId.Value == Guid.Empty)
            return Error.Validation("Please select a branch to continue.");

        var items = await _dbContext.Set<RegistrationEntity>()
            .AsNoTracking()
            .Where(r => r.BranchId == branchId.Value && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .ProjectToType<RegistrationLightResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success(items);
    }
}
