using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Commands.StaffDocument.PatchStaffDocument;

public class PatchStaffDocumentCommandValidator : AbstractValidator<PatchStaffDocumentCommand>
{
    public PatchStaffDocumentCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.DocumentType).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Request.DocumentType));
        RuleFor(x => x.Request.FileUrl).MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Request.FileUrl));
        RuleFor(x => x.Request.VerificationStatus)
            .Must(v => new[] { "not_verified", "verified", "rejected" }.Contains(v))
            .When(x => !string.IsNullOrEmpty(x.Request.VerificationStatus));
    }
}
