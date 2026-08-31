using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Commands.UpdateBranch;

public class UpdateBranchValidator : AbstractValidator<UpdateBranchCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentTenantService _tenantService;

    public UpdateBranchValidator(
        IApplicationDbContext dbContext,
        ICurrentTenantService tenantService)
    {
        _dbContext = dbContext;
        _tenantService = tenantService;

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Request.Code)
            .NotEmpty()
            .MaximumLength(50)
            .Matches("^[A-Z0-9]+$")
            .WithMessage("Code must be uppercase alphanumeric.")
            .MustAsync(async (cmd, code, ct) =>
            {
                var tenantId = _tenantService.GetTenantId();

                return !await _dbContext.Set<Branch>()
                    .AnyAsync(
                        b => b.TenantId == tenantId
                             && b.Code == code
                             && b.Id != cmd.BranchId
                             && !b.IsDeleted,
                        ct);
            })
            .WithMessage(x => $"Branch code '{x.Request.Code}' already exists.");

        RuleFor(x => x.Request.Status)
            .Must(s => new[] { "active", "suspended", "closed" }.Contains(s))
            .WithMessage("Status must be active, suspended, or closed.");
    }
}