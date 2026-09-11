using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Commands.StaffDocument.CreateStaffDocument;

public class CreateStaffDocumentCommandValidator : AbstractValidator<CreateStaffDocumentCommand>
{
    public CreateStaffDocumentCommandValidator()
    {
        RuleFor(x => x.Request.StaffId).GreaterThan(0);
        RuleFor(x => x.Request.DocumentType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.FileUrl).NotEmpty().MaximumLength(500);
    }
}
