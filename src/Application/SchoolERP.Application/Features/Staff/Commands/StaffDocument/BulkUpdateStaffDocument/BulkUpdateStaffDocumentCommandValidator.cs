using FluentValidation;

namespace SchoolERP.Application.Features.Staff.Commands.StaffDocument.BulkUpdateStaffDocument;

public class BulkUpdateStaffDocumentCommandValidator : AbstractValidator<BulkUpdateStaffDocumentCommand>
{
    public BulkUpdateStaffDocumentCommandValidator()
    {
        RuleFor(x => x.Request.Ids).NotEmpty();
        RuleFor(x => x.Request.DocumentType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Request.VerificationStatus)
            .Must(v => new[] { "not_verified", "verified", "rejected" }.Contains(v));
    }
}
