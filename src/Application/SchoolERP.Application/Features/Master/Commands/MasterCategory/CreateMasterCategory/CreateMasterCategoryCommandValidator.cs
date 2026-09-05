using FluentValidation;

namespace SchoolERP.Application.Features.Master.Commands.MasterCategory.CreateMasterCategory;

public class CreateMasterCategoryCommandValidator : AbstractValidator<CreateMasterCategoryCommand>
{
    public CreateMasterCategoryCommandValidator()
    {
        RuleFor(x => x.Request.ModuleId).GreaterThan(0);
        RuleFor(x => x.Request.Key).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Description).MaximumLength(255).When(x => x.Request.Description != null);
    }
}