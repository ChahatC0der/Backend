using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Branches.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Commands.UpdateBranch;

public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, Result<BranchResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateBranchCommandHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<BranchResponse>> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _dbContext.Set<Branch>()
            .FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == request.TenantId && !b.IsDeleted, cancellationToken);

        if (branch == null)
            return Error.NotFound("Branch", request.BranchId.ToString());

        // Code uniqueness (exclude current)
        if (branch.Code != request.Request.Code)
        {
            var codeExists = await _dbContext.Set<Branch>()
                .AnyAsync(b => b.TenantId == request.TenantId && b.Code == request.Request.Code && b.Id != request.BranchId && !b.IsDeleted, cancellationToken);
            if (codeExists)
                return Error.Conflict($"Branch code '{request.Request.Code}' already exists.");
        }

        // If setting as Default, reset other defaults
        if (request.Request.IsDefault)
        {
            var existingDefaults = await _dbContext.Set<Branch>()
                .Where(b => b.TenantId == request.TenantId && b.IsDefault && b.Id != request.BranchId)
                .ToListAsync(cancellationToken);
            foreach (var item in existingDefaults)
                item.IsDefault = false;
        }

        branch.Name = request.Request.Name;
        branch.Code = request.Request.Code;
        branch.Address = request.Request.Address;
        branch.Phone = request.Request.Phone;
        branch.Email = request.Request.Email;
        branch.ContactPerson = request.Request.ContactPerson;
        branch.IsDefault = request.Request.IsDefault;
        branch.Status = request.Request.Status;
        branch.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(branch.Adapt<BranchResponse>(), "Branch updated successfully.");
    }
}