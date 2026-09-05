using FluentValidation;

public class UpdateSectionCommandValidator : AbstractValidator<UpdateSectionCommand>
{
    public UpdateSectionCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.ClassId).GreaterThan(0);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Request.Capacity).GreaterThan(0).When(x => x.Request.Capacity.HasValue);
    }
}