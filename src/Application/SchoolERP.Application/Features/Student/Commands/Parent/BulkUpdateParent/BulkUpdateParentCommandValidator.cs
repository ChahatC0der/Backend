using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Parent.BulkUpdateParent;

public class BulkUpdateParentCommandValidator : AbstractValidator<BulkUpdateParentCommand>
{
    public BulkUpdateParentCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
    }
}
