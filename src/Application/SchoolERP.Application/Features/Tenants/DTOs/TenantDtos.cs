namespace SchoolERP.Application.Features.Tenants.DTOs;

public record CreateTenantRequest(
    string Name,
    string ContactEmail,
    string? ContactPhone,
    string? Address,
    string Plan
);

public record TenantResponse(
    Guid Id,
    string Name,
    string Subdomain,
    string ContactEmail,
    string Plan,
    string Status,
    int StudentCount,
    DateTime CreatedAt
);