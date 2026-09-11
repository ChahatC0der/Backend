using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Commands.StaffDocument.UpdateStaffDocument;

public class UpdateStaffDocumentCommandValidator : AbstractValidator<UpdateStaffDocumentCommand>
{
    public UpdateStaffDocumentCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.StaffId).GreaterThan(0);
        RuleFor(x => x.Request.DocumentType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.FileUrl).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Request.VerificationStatus)
            .Must(v => new[] { "not_verified", "verified", "rejected" }.Contains(v));
    }
}
