using FluentValidation;

public class UpdateClassGroupCommandValidator : AbstractValidator<UpdateClassGroupCommand>
{
    public UpdateClassGroupCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Sequence).GreaterThan((byte)0);
        RuleFor(x => x.Request.Description).MaximumLength(255).When(x => x.Request.Description != null);
    }
}