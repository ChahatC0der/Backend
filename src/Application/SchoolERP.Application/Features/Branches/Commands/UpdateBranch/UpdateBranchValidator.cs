using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Commands.UpdateBranch;

public class UpdateBranchValidator : AbstractValidator<UpdateBranchCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateBranchValidator(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(x => x.Request.Name)
            .NotEmpty().MaximumLength(255);

        RuleFor(x => x.Request.Code)
            .NotEmpty().MaximumLength(50)
            .Matches("^[A-Z0-9]+$").WithMessage("Code must be uppercase alphanumeric.")
            .MustAsync(async (cmd, code, ct) =>
                !await _dbContext.Set<Branch>()
                    .AnyAsync(b => b.TenantId == cmd.TenantId && b.Code == code && b.Id != cmd.BranchId && !b.IsDeleted, ct))
            .WithMessage(x => $"Branch code '{x.Request.Code}' already exists.");

        RuleFor(x => x.Request.Status)
            .Must(s => new[] { "active", "suspended", "closed" }.Contains(s))
            .WithMessage("Status must be active, suspended, or closed.");
    }
}