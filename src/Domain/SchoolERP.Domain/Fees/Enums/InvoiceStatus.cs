namespace SchoolERP.Domain.Fees.Enums;

public enum InvoiceStatus
{
    Draft         = 1,
    Sent          = 2,
    PartiallyPaid = 3,
    Paid          = 4,
    Overdue       = 5,
    Cancelled     = 6,
}
