using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Extensions; // 👈 HELPER YAHAN HAI
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Branches.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Queries.GetBranchById;

public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, Result<BranchResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetBranchByIdQueryHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<BranchResponse>> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
    {
        // 1️⃣ 🔥 GET USING HELPER (NO MANUAL NULL CHECK)
        var branchResult = await _dbContext.GetEntityByIdAsync<Branch>(request.BranchId, cancellationToken);
        if (branchResult.IsFailure)
            return branchResult.Error;

        var branch = branchResult.Value;

        // 2️⃣ 🔥 MANUAL MAPPING WITH UPDATEDAT
        var response = new BranchResponse(
            branch.Id,
            branch.TenantId,
            branch.Name,
            branch.Code,
            branch.Address,
            branch.Phone,
            branch.Email,
            branch.ContactPerson,
            branch.IsDefault,
            branch.Status,
            branch.CreatedAt,
            branch.UpdatedAt
        );

        return Result.Success(response);
    }
}