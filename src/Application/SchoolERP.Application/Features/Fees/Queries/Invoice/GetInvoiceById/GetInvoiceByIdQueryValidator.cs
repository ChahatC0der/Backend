using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Queries.Invoice.GetInvoiceById;

public sealed class GetInvoiceByIdQueryValidator : AbstractValidator<GetInvoiceByIdQuery>
{
    public GetInvoiceByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
