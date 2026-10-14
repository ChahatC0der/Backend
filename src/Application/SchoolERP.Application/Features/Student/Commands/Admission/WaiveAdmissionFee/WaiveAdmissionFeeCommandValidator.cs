using FluentValidation;

namespace SchoolERP.Application.Features.Student.Commands.Admission.WaiveAdmissionFee;

public class WaiveAdmissionFeeCommandValidator : AbstractValidator<WaiveAdmissionFeeCommand>
{
    public WaiveAdmissionFeeCommandValidator()
    {
        RuleFor(x => x.Request.Id).GreaterThan(0);
        RuleFor(x => x.Request.Reason).NotEmpty().MaximumLength(500);
    }
}
