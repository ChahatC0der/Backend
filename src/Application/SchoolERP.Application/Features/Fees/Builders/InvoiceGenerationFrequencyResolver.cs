
using SchoolERP.Domain.Fees.Enums;

namespace SchoolERP.Application.Features.Fees.Builders;

/// <summary>
/// Resolves the invoice generation frequency for a tenant.
///
/// Current implementation:
/// - Frequency is hard-coded per tenant.
/// - The same frequency applies to all branches of that tenant.
/// - No database configuration is required.
///
/// Future:
/// This class can read tenant-specific settings from
/// the Settings Module without changing billing calculations.
/// </summary>
public static class InvoiceGenerationFrequencyResolver
{
    // BLOCK 1: Maintain tenant-specific frequency selections here.
    //
    // Add each tenant's ID and its required invoice frequency.
    // All four frequencies are supported.
    //
    // Example:
    // [Guid.Parse("YOUR-TENANT-ID")] = InvoiceGenerationFrequency.Quarterly
    //
    // Keep this mapping centralized rather than scattering
    // frequency constants across command handlers.

    private static readonly IReadOnlyDictionary<Guid, InvoiceGenerationFrequency>
        TenantFrequencies =
            new Dictionary<Guid, InvoiceGenerationFrequency>
            {
                // Add tenant-specific mappings here.
            };

    // BLOCK 2: Define the temporary default.
    //
    // Tenants without an explicit mapping will use Monthly.
    // Change this default only when that is the intended
    // behaviour for unmapped tenants.
    private const InvoiceGenerationFrequency DefaultFrequency =
        InvoiceGenerationFrequency.Monthly;

    /// <summary>
    /// Gets the invoice generation frequency for a tenant.
    ///
    /// TenantId is used instead of BranchId because the selected
    /// frequency must remain consistent across all branches
    /// belonging to the same tenant.
    /// </summary>
    public static InvoiceGenerationFrequency Resolve(Guid tenantId)
    {
        // BLOCK 3: Reject invalid tenant context.
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException(
                "TenantId is required to resolve invoice generation frequency.",
                nameof(tenantId));
        }

        // BLOCK 4: Return explicitly configured tenant frequency.
        if (TenantFrequencies.TryGetValue(
                tenantId,
                out var frequency))
        {
            return frequency;
        }

        // BLOCK 5: Use the temporary default for unmapped tenants.
        return DefaultFrequency;
    }
}
