using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.Concession.AllocateConcession;

public sealed class AllocateConcessionCommandValidator : AbstractValidator<AllocateConcessionCommand>
{
    public AllocateConcessionCommandValidator()
    {
        RuleFor(x => x.Request.ConcessionId).GreaterThan(0);
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request).Must(r => r.StudentId.HasValue ^ r.ClassId.HasValue)
            .WithMessage("Provide exactly one of StudentId or ClassId.");
        RuleFor(x => x.Request.SectionId).Null().When(x => x.Request.StudentId.HasValue)
            .WithMessage("SectionId is only valid with ClassId.");
    }
}
