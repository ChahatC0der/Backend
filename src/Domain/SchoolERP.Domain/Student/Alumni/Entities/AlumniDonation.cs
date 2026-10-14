using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Student.Alumni.Entities;

public class AlumniDonation : BaseEntity
{
    public long AlumniId { get; set; }
    public DateOnly DonationDate { get; set; }
    public decimal Amount { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string PaymentMode { get; set; } = string.Empty;
    public string ReceiptNumber { get; set; } = string.Empty;
    public bool IsAnonymous { get; set; }
    public string? Notes { get; set; }
    public long? RecordedBy { get; set; }

    public Alumni Alumni { get; set; } = null!;
}
