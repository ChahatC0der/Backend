using FluentValidation;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Branches.Commands.CreateBranch;

public class CreateBranchValidator : AbstractValidator<CreateBranchCommand>
{
    private readonly IQueryable<Branch> _branches;

    public CreateBranchValidator(IApplicationDbContext dbContext)
    {
        _branches = dbContext.Set<Branch>();

        RuleFor(x => x.Request.Name)
            .NotEmpty().MaximumLength(255);

        RuleFor(x => x.Request.Code)
            .NotEmpty().MaximumLength(50)
            .Matches("^[A-Z0-9]+$").WithMessage("Code must be uppercase alphanumeric.")
            //.MustAsync(async (cmd, code, ct) =>
               // await _branches.IsUniqueAsync(nameof(Branch.Code), code, cancellationToken: ct))
            .WithMessage("Branch code is already taken.");
    }
}