using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Alumni.UpdateAlumni;

public class UpdateAlumniCommandValidator : AbstractValidator<UpdateAlumniCommand>
{
    public UpdateAlumniCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.FirstName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Request.CurrentStatus).NotEmpty()
            .Must(s => new[] { "working", "higher_studies", "self_employed", "not_employed", "retired", "deceased" }.Contains(s));
    }
}
