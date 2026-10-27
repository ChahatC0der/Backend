using FluentValidation;

namespace SchoolERP.Application.Features.Transport.Commands.TransportFee.DeleteTransportFee;

public sealed class DeleteTransportFeeCommandValidator : AbstractValidator<DeleteTransportFeeCommand>
{
    public DeleteTransportFeeCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
