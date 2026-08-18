namespace SchoolERP.Application.Features.Tenants.DTOs;

// 🔥 CREATE
public record CreateTenantRequest(
    string Code,
    string Name,
    string Subdomain,
    string ContactEmail,
    string? ContactPhone = null,
    string? Address = null,
    string Plan = "basic"
);

// 🔥 UPDATE (Full)
public record UpdateTenantRequest(
    string Code,
    string Name,
    string Subdomain,
    string ContactEmail,
    string? ContactPhone = null,
    string? Address = null,
    string Plan = "basic",
    string Status = "active"
);

// 🔥 BULK UPDATE REQUEST
public record BulkUpdateTenantRequest(
    List<Guid> Ids,
    string? Code = null,
    string? Name = null,
    string? Subdomain = null,
    string? ContactEmail = null,
    string? ContactPhone = null,
    string? Address = null,
    string? Plan = null,
    string? Status = null
);

// 🔥 PATCH (Partial Update — Sirf jo bheja jaye woh update ho)
public record PatchTenantRequest(
    string? Code = null,
    string? Name = null,
    string? Subdomain = null,
    string? ContactEmail = null,
    string? ContactPhone = null,
    string? Address = null,
    string? Plan = null,
    string? Status = null
);

// 🔥 RESPONSE
public record TenantResponse(
    Guid Id,
    string Code,
    string Name,
    string Subdomain,
    string ContactEmail,
    string Plan,
    string Status,
    int StudentCount,
    DateTime CreatedAt
);

// 🔥 LIGHTWEIGHT RESPONSE (Dropdowns ke liye)
public record TenantLightResponse(
    Guid Id,
    string Code,
    string Name
);

// 🔥 BULK DELETE REQUEST
public record BulkDeleteRequest(List<Guid> Ids);