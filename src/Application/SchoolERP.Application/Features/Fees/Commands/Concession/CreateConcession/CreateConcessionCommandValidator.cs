using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Fees.Enums;
using ConcessionEntity = SchoolERP.Domain.Fees.Entities.Concession;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.CreateConcession;

public sealed class CreateConcessionCommandValidator : AbstractValidator<CreateConcessionCommand>
{
    public CreateConcessionCommandValidator(IApplicationDbContext db, ICurrentTenantService tenant, ICurrentBranchService branch)
    {
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(150)
            .MustAsync(async (cmd, name, ct) =>
            {
                var scope = FeeScopeGuard.RequireBranch(tenant, branch);
                if (scope.IsFailure) return true;   // handler error dikhayega
                var (tenantId, branchId) = scope.Value;
                return !await db.Set<ConcessionEntity>().AsNoTracking().AnyAsync(c =>
                    c.TenantId == tenantId && c.BranchId == branchId
                    && c.Name == name.Trim() && c.DeletedAt == null, ct);
            })
            .WithMessage(x => $"Concession with name '{x.Request.Name}' already exists.");

        RuleFor(x => x.Request.Category).IsInEnum();
        RuleFor(x => x.Request.CalculationType).IsInEnum();
        RuleFor(x => x.Request.Value).GreaterThan(0);
        RuleFor(x => x.Request.Value)
            .LessThanOrEqualTo(100)
            .When(x => x.Request.CalculationType == ConcessionCalculationType.Percentage)
            .WithMessage("Percentage concession cannot exceed 100.");
        RuleFor(x => x.Request.EndDate)
            .GreaterThanOrEqualTo(x => x.Request.StartDate)
            .When(x => x.Request.StartDate.HasValue && x.Request.EndDate.HasValue);
    }
}