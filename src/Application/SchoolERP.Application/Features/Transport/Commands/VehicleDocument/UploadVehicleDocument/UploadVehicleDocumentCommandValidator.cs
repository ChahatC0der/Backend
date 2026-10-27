using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.VehicleDocument.UploadVehicleDocument;

public sealed class UploadVehicleDocumentCommandValidator : AbstractValidator<UploadVehicleDocumentCommand>
{
    public UploadVehicleDocumentCommandValidator()
    {
        RuleFor(x => x.VehicleId).GreaterThan(0);
        RuleFor(x => x.Request.DocumentType)
            .Must(t => t is "insurance" or "puc" or "fitness" or "registration")
            .WithMessage("DocumentType must be one of: insurance, puc, fitness, registration.");
        RuleFor(x => x.Request.FileUrl).NotEmpty().MaximumLength(500);
    }
}
