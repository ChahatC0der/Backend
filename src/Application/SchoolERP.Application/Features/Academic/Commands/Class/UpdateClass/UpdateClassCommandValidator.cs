using FluentValidation;

public class UpdateClassCommandValidator : AbstractValidator<UpdateClassCommand>
{
    public UpdateClassCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Sequence).GreaterThan((byte)0);
        RuleFor(x => x.Request.ClassGroupId).GreaterThan(0).When(x => x.Request.ClassGroupId.HasValue);
    }
}