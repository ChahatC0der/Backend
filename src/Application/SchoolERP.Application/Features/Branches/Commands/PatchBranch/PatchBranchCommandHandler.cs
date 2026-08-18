using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Branches.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Commands.PatchBranch;

public class PatchBranchCommandHandler : IRequestHandler<PatchBranchCommand, Result<BranchResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public PatchBranchCommandHandler(IApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<Result<BranchResponse>> Handle(PatchBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _dbContext.Set<Branch>()
            .FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == request.TenantId && !b.IsDeleted, cancellationToken);

        if (branch == null)
            return Error.NotFound("Branch", request.BranchId.ToString());

        // Code uniqueness check (if code is being updated)
        if (!string.IsNullOrEmpty(request.Request.Code) && branch.Code != request.Request.Code)
        {
            var codeExists = await _dbContext.Set<Branch>()
                .AnyAsync(b => b.TenantId == request.TenantId && b.Code == request.Request.Code && b.Id != request.BranchId && !b.IsDeleted, cancellationToken);
            if (codeExists)
                return Error.Conflict($"Branch code '{request.Request.Code}' already exists.");
        }

        // If setting default, reset other defaults
        if (request.Request.IsDefault.HasValue && request.Request.IsDefault.Value)
        {
            var defaults = await _dbContext.Set<Branch>()
                .Where(b => b.TenantId == request.TenantId && b.IsDefault && b.Id != request.BranchId)
                .ToListAsync(cancellationToken);
            foreach (var d in defaults) d.IsDefault = false;
        }

        // Patch fields
        if (!string.IsNullOrEmpty(request.Request.Name)) branch.Name = request.Request.Name;
        if (!string.IsNullOrEmpty(request.Request.Code)) branch.Code = request.Request.Code;
        if (!string.IsNullOrEmpty(request.Request.Address)) branch.Address = request.Request.Address;
        if (!string.IsNullOrEmpty(request.Request.Phone)) branch.Phone = request.Request.Phone;
        if (!string.IsNullOrEmpty(request.Request.Email)) branch.Email = request.Request.Email;
        if (!string.IsNullOrEmpty(request.Request.ContactPerson)) branch.ContactPerson = request.Request.ContactPerson;
        if (request.Request.IsDefault.HasValue) branch.IsDefault = request.Request.IsDefault.Value;
        if (!string.IsNullOrEmpty(request.Request.Status)) branch.Status = request.Request.Status;

        branch.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(branch.Adapt<BranchResponse>(), "Branch updated successfully.");
    }
}