using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Commands.StaffDocument.BulkPatchStaffDocument;

public class BulkPatchStaffDocumentCommandValidator : AbstractValidator<BulkPatchStaffDocumentCommand>
{
    public BulkPatchStaffDocumentCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.DocumentType).MaximumLength(50).When(x => !string.IsNullOrEmpty(x.Request.DocumentType));
        RuleFor(x => x.Request.VerificationStatus)
            .Must(v => new[] { "not_verified", "verified", "rejected" }.Contains(v))
            .When(x => !string.IsNullOrEmpty(x.Request.VerificationStatus));
    }
}
