using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Tenants.Entities;

public class Tenant : GuidAuditableEntity
{
    // 🔥 1. Identity & Branding
    public string Code { get; set; } = string.Empty; // Unique, used in Invoice prefix
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }

    // 🔥 2. Contact
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }

    // 🔥 3. Owner
    public string? OwnerName { get; set; }
    public string? OwnerEmail { get; set; }
    public string? OwnerPhone { get; set; }
    public string? OwnerDesignation { get; set; }

    // 🔥 4. Legal & Compliance
    public string? Gstin { get; set; }
    public string? Pan { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Affiliation { get; set; } // CBSE, ICSE, IB, State Board

    // 🔥 5. Localization & Financial
    public string Currency { get; set; } = "INR";
    public string TimeZone { get; set; } = "India Standard Time";
    public string Plan { get; set; } = "basic";
    public string Status { get; set; } = "active";

    // 🔥 6. Subscription
    public DateTime? TrialEndsAt { get; set; }
    public DateTime? SubscriptionEndsAt { get; set; }

    // 🔥 7. JSON Config (Flexible)
    public string? Settings { get; set; } // Module ON/OFF, custom fields etc.
    public string? CustomFieldsDef { get; set; }

    // 🔥 8. Denormalized Stats (for quick dashboard)
    public int StudentCount { get; set; }
    public long StorageUsedMb { get; set; }
    public long ApiCallsMonth { get; set; }

    // 🔥 9. Navigation (One-to-Many)
    //public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    //public ICollection<TenantBankDetail> BankDetails { get; set; } = new List<TenantBankDetail>();
}