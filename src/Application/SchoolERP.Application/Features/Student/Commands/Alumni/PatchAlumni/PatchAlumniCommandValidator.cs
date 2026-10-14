using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Alumni.PatchAlumni;

public class PatchAlumniCommandValidator : AbstractValidator<PatchAlumniCommand>
{
    public PatchAlumniCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.FirstName).MaximumLength(255).When(x => !string.IsNullOrEmpty(x.Request.FirstName));
        RuleFor(x => x.Request.CurrentStatus)
            .Must(s => new[] { "working", "higher_studies", "self_employed", "not_employed", "retired", "deceased" }.Contains(s))
            .When(x => !string.IsNullOrEmpty(x.Request.CurrentStatus));
    }
}
