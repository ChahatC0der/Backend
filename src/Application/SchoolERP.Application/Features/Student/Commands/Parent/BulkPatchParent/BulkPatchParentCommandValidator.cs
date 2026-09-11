using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Parent.BulkPatchParent;

public class BulkPatchParentCommandValidator : AbstractValidator<BulkPatchParentCommand>
{
    public BulkPatchParentCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
    }
}
