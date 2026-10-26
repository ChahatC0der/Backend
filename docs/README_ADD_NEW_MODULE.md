# SchoolERP Backend — Architecture & New-Module Guide for AI Agents

> **Purpose:** This document describes the backend found in the supplied source archive. Use it as repository-specific context when adding a new SchoolERP module with ChatGPT, DeepSeek, Copilot, or another coding agent. It records the existing conventions and helpers to reuse; it is not a proposal to replace the current architecture.

## 1. Non-negotiable instructions for an AI coding agent

Before changing code:

1. Read this README, then inspect the closest existing feature that does the same job. For ordinary CRUD + Excel export, start with **AcademicYear**; for simpler master-data CRUD, compare **MasterCategory/MasterItem**; for student workflows, inspect **Registration/Enquiry**.
2. Follow the current four-layer project structure and existing CQRS/result patterns. Do not introduce a new repository, service abstraction, mapper, Excel library, generic CRUD framework, or package unless the current code truly cannot support the requirement.
3. Use `IApplicationDbContext` and `Set<TEntity>()` in Application handlers. Use `AppDbContext` only through the existing interface in feature handlers; do not inject `AppDbContext` directly into an Application feature.
4. Reuse `IExcelExportService` / `ClosedXmlExportService` for `.xlsx` exports. It is already registered in Infrastructure. Do not create a second Excel export service.
5. Decide whether data is platform-, tenant-, or branch-scoped before creating the entity. Never trust a request-body `TenantId` or `BranchId` as authorization. Scope IDs from the current request context and validate relationships.
6. Do not copy `[AllowAnonymous]` from most existing CRUD controllers. In the current files those attributes are generally marked `TODO: [HasPermission(...)]`; they are not a secure authorization template.
7. Do not assume soft-delete or tenant isolation is automatically correct everywhere. See **Known implementation caveats** below and explicitly protect each new query/write.
8. Keep changes minimal and consistent with the nearest working implementation. Build the API and review all compiler errors after making the module. The supplied archive did not contain a solution file, test project, or EF migration directory, so inspect the real repository before assuming those are absent there.

## 2. Architecture at a glance

The source has four main project areas under `src/`:

```text
src/
├── API/SchoolERP.API/                         # HTTP endpoints and startup
│   ├── Controllers/
│   ├── Authorization/
│   ├── Middleware/
│   ├── Filters/
│   ├── Program.cs
│   └── DependencyInjection.cs
├── Application/SchoolERP.Application/         # Use cases / CQRS / DTOs
│   ├── Common/
│   │   ├── Abstractions/
│   │   ├── Behaviors/
│   │   ├── DTOs/
│   │   ├── Extensions/
│   │   ├── Interfaces/
│   │   └── ...
│   ├── Features/<Area>/Commands/<Entity>/<Action>/
│   ├── Features/<Area>/Queries/<Entity>/<Action>/
│   └── DependencyInjection.cs
├── Domain/SchoolERP.Domain/                   # Business entities
│   ├── Common/
│   ├── Academic/Entities/
│   ├── Master/Entities/
│   ├── Student/...
│   ├── Staff/Entities/
│   ├── Tenants/Entities/
│   └── Rbac/Entities/
├── Domain/SchoolERP.Domain.Shared/            # Result/Error primitives
└── Infrastructure/SchoolERP.Infrastructure/   # EF Core, SQL, external services
    ├── Data/
    ├── Persistence/
    │   ├── AppDbContext.cs
    │   ├── TenantDbContext.cs
    │   └── Configurations/<Area>/
    ├── MultiTenancy/
    ├── Identity/
    ├── Services/
    └── DependencyInjection.cs
```

### Request and data flow

```text
HTTP request
  -> API Controller
  -> IMediator.Send(Command or Query)
  -> MediatR pipeline (logging, validation, performance, transaction for commands)
  -> Command/Query Handler
  -> IApplicationDbContext.Set<TEntity>() / EF Core
  -> AppDbContext -> SQL Server
  -> Result<T> or Result<byte[]>
  -> Controller HandleResult(...) or File(...)
  -> HTTP response
```

- **API** owns routing, HTTP status codes, authorization attributes, and file responses.
- **Application** owns use cases, requests/handlers, validators, DTOs, and abstractions.
- **Domain** owns entities and base entity/tenant/branch markers.
- **Infrastructure** implements persistence, identity, tenant context, caching, Dapper, Excel export, ID generators, and other external integrations.
- `SchoolERP.Domain` references `SchoolERP.Domain.Shared`. Application and Infrastructure reference Domain; API references Application and Infrastructure.

## 3. Technology and packages already present

All five project files target **.NET 10 (`net10.0`)**.

| Concern | Existing technology / package |
|---|---|
| HTTP API | ASP.NET Core Controllers, OpenAPI/Swagger, Scalar |
| Authentication | JWT Bearer, ASP.NET Core Identity with `long` user/role IDs, OpenIddict packages |
| Authorization | custom dynamic policy provider, `HasPermissionAttribute`, permission handler/service |
| Use cases / CQRS | MediatR 12.4.1 |
| Validation | FluentValidation 12.1.1; validators are assembly-scanned |
| Mapping | Mapster 10.0.11 (`Adapt<T>()`, `ProjectToType<T>()`) |
| ORM / database | EF Core SQL Server 10.0.10, Microsoft.Data.SqlClient 7.0.2 |
| Optimized SQL reads | Dapper 2.1.79 (use cautiously; see caveats) |
| Tenant resolution | Finbuckle.MultiTenant 10.x |
| Excel | ClosedXML 0.105.1, through the existing export service |
| Logging / diagnostics | Serilog, OpenTelemetry, SQL/ASP.NET instrumentation, health checks |
| Other available integrations | Hangfire, AWS S3 SDK, in-memory caching, AI/OpenRouter abstractions and guardrails |

**Do not add a package just to do something that an existing helper already does.** Check the project file and existing services first.

## 4. CQRS, `Result<T>`, and pipeline rules

### Commands and queries

Location: `Application/SchoolERP.Application/Common/Abstractions/ICommand.cs`

- `ICommand<TResponse>` represents a write command and produces `Result<TResponse>`. It implements the `ICommandBase` marker, which is how `TransactionBehavior` identifies writes.
- `IQuery<TResponse>` represents a read query and also produces `Result<TResponse>`.
- Handlers generally implement `IRequestHandler<TRequest, Result<TResponse>>`.
- Use a command for create/update/patch/delete/restore/bulk mutations. Use a query for read/list/detail/export operations.
- A request validator should inherit from `AbstractValidator<TCommand>` or `AbstractValidator<TQuery>` and live beside that request. Validators are scanned automatically by `AddApplication()`.

### Registered MediatR behaviors

`Application/SchoolERP.Application/DependencyInjection.cs` registers:

1. `LoggingBehavior<,>`
2. `ValidationBehavior<,>`
3. `PerformanceBehavior<,>`
4. `TransactionBehavior<,>`

The transaction behavior starts a transaction for an `ICommandBase` request, calls the handler, then calls `IApplicationDbContext.SaveChangesAsync()` and commits. It rolls back when an exception is thrown. **Do not normally call `SaveChangesAsync()` inside a command handler**; let this pipeline own persistence. The transaction behavior does not inspect a returned `Result.IsSuccess`, so validate and check all failure cases before mutating tracked entities.

### Result and API response handling

Location: `Domain/SchoolERP.Domain.Shared/Results/Result.cs`.

Use `Result.Success(value, optionalMessage)`, `Result.Failure<T>(error)`, or return an `Error` from handlers using the established implicit conversion. Existing error factories include:

- `Error.NotFound(entityName, id)`
- `Error.Validation(message)`
- `Error.Conflict(message)`
- `Error.Unauthorized(message)`

`API/SchoolERP.API/Controllers/BaseApiController.cs` exposes `HandleResult(...)`, which maps successful results to a standard success response and maps error codes to HTTP responses (NotFound → 404, Validation → 400, Unauthorized → 401, Conflict → 409, otherwise 500). Controllers should use this instead of inventing a different response envelope for each endpoint. Excel download endpoints are the exception on success: after checking the result, return `File(...)` with the Excel content type and filename.

## 5. Domain base entities: choose the correct one

Location: `Domain/SchoolERP.Domain/Common/BaseEntity.cs` and `BaseAuditableEntity.cs`.

| Base type | Current meaning | Appropriate use |
|---|---|---|
| `BaseEntity` | `long Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`, `DeletedAt`; **no tenant or branch marker by itself** | A long-ID entity with common audit/soft-delete fields, only when its scope is handled deliberately elsewhere |
| `GuidAuditableEntity` | `Guid Id`, timestamps and soft-delete fields | Current pattern used by Tenant/Branch-style GUID entities |
| `BaseTenantEntity` | Extends `BaseEntity`; adds `Guid TenantId` and `IMustHaveTenant` | Tenant-owned entity not tied to an individual branch |
| `BranchEntity` | Extends `BaseEntity`; adds `Guid BranchId` and `IMustHaveBranch`; does **not** add `TenantId` | Existing branch-only pattern, e.g. the current `AcademicYear` model; use only with deliberate branch-to-tenant validation/isolation |
| `BaseBranchEntity` | Extends `BaseTenantEntity`; adds `Guid BranchId` and `IMustHaveBranch` | Preferred base for ordinary business data scoped to **both tenant and branch** |
| `BaseAuditLog` | Separate audit-log shape: long ID, nullable TenantId in the CLR model, PerformedBy, CreatedAt | Audit history entities using the matching audit configuration |
| `TenantAuditableEntity` | Extends `BaseAuditableEntity`, adds TenantId and declares CreatedAt | Exists in source but is a less common/legacy pattern; inspect consumers before choosing it |

`IAuditableTimestamps`, `IMustHaveTenant`, and `IMustHaveBranch` are important marker interfaces. `Student` currently implements both tenant and branch scope; `AcademicYear` currently implements branch scope only. Do not select a base type just because its name looks convenient: inspect the data-ownership requirement first.

### EF configuration base classes

Under `Infrastructure/SchoolERP.Infrastructure/Persistence/Configurations/Common/`:

- `BaseEntityConfiguration<TEntity>` configures the long primary key, `IsDeleted` default, `CreatedAt` SQL default, and nullable `UpdatedAt`/`DeletedAt`.
- `BaseGuidEntityConfiguration<TEntity>` configures a GUID key as `ValueGeneratedNever()`, `IsDeleted` default and required/optional audit columns.
- `BaseTenantEntityConfiguration<TEntity>` extends the base entity configuration and configures/indexes `TenantId`.
- `BaseBranchEntityConfiguration<TEntity>` extends tenant configuration and configures/indexes `BranchId`; its generic constraint expects an entity that includes both tenant and branch markers.
- `BaseBranchOnlyEntityConfiguration<TEntity>` extends base entity configuration and configures/indexes `BranchId`; it does not add a TenantId column.
- `BaseAuditLogConfiguration<TEntity>` configures common audit key/columns and indexes.

Every new domain entity should have an explicit `IEntityTypeConfiguration<TEntity>` file in the Infrastructure configuration folder. Inherit from the appropriate base configuration and then configure table name, lengths, required/optional columns, relationships, delete behavior, indexes and unique constraints.

### Model registration and saving

`Infrastructure/SchoolERP.Infrastructure/Persistence/AppDbContext.cs`:

- `AppDbContext` derives from `IdentityDbContext<ApplicationUser, IdentityRole<long>, long>` and implements `IApplicationDbContext`.
- It calls `modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly)`, so Infrastructure entity configurations in that assembly are discovered automatically.
- Application handlers use `_dbContext.Set<TEntity>()`; a named `DbSet<TEntity>` property is useful for readability but is not required by the `IApplicationDbContext` contract when the entity is in the EF model.
- `SaveChangesAsync()` stamps `TenantId` for added `IMustHaveTenant` entities, `BranchId` for added `IMustHaveBranch` entities, and timestamps for entities implementing `IAuditableTimestamps`.
- Added tenant-scoped rows fail if the current tenant is unresolved; added branch-scoped rows fail if the current branch is unresolved. Do not manually set these scope values from untrusted client input.
- `TenantDbContext` is a separate plain `DbContext` with a `DbSet<Tenant>` and is used by tenant-store plumbing. Its current `ApplyConfigurationsFromAssembly(...)` call discovers configuration classes from the same assembly; verify whether this is intentional before changing tenant-store or migration behavior.

## 6. Current tenant, branch, and user context

Abstractions are in `Application/Common/Interfaces/`; implementations are in `Infrastructure/Services/`.

- `ICurrentTenantService` → `CurrentTenantService`, reads the Finbuckle tenant context and parses `TenantInfo.Id` into a `Guid`.
- `ICurrentBranchService` → `CurrentBranchService`, reads the `BranchId` request header first, then the `BranchId` JWT claim. `GetBranchId()` may return null.
- `ICurrentUserService` → `CurrentUserService`, reads authenticated user ID from `ClaimTypes.NameIdentifier` or `sub` and returns `long?`.
- Finbuckle is registered in `Infrastructure/DependencyInjection.cs` with host strategy and a `TenantId` header strategy. The API pipeline calls `UseMultiTenant()`.

For a tenant-and-branch module, handlers should require a resolved branch when appropriate and filter/look up records using the active scope. Use the current services and ensure a selected branch belongs to the authenticated tenant. A valid GUID in a header is not, by itself, proof of authorization.

## 7. Existing helpers and services to reuse

### Database and validation helpers

| Existing helper/interface | Use |
|---|---|
| `IApplicationDbContext` | EF Core abstraction for `Set<TEntity>()`, `SaveChangesAsync()` and transaction methods |
| `EntityExistsExtensions.EnsureEntityExistsAsync<TEntity>(...)` | Check whether an ID exists; returns an `Error?` |
| `EntityExistsExtensions.GetEntityByIdAsync<TEntity>(...)` | Fetch by ID as `Result<TEntity>` |
| `EntityExistsExtensions.GetEntityAsync<TEntity>(predicate, ...)` | Fetch by a scope-aware predicate as `Result<TEntity>`; **does not automatically filter `IsDeleted`** |
| `UniquenessCheckExtensions.EnsureUniqueAsync<TEntity>(...)` | One duplicate check; returns `Error.Conflict(...)` or null |
| `UniquenessCheckExtensions.EnsureAllUniqueAsync<TEntity>(...)` | Several duplicate checks and combines conflict messages |
| `PatchHelperExtensions.PatchIfProvided(...)` | Applies a non-null/non-empty string value after trimming; for nullable numbers/dates/booleans, handle `HasValue` explicitly |
| `PagedRequest` / `PagedResponse<T>` | Standard paging DTOs; request has Page, Size, SortBy, SortOrder, SearchTerm; response has Data, Page, Size, TotalCount and TotalPages |
| `Mapster` | Existing `Adapt<T>()` mapping and `ProjectToType<T>()` query projection |
| `ICurrentTenantService`, `ICurrentBranchService`, `ICurrentUserService` | Current scope and authenticated-user context |
| `IStaffIdGenerator`, `IEnrollmentIdGenerator` | Existing ID-generation services for matching staff/enrollment use cases |
| `ICacheService` | Existing memory-cache abstraction |
| `IPermissionService` / `HasPermissionAttribute` | Existing permission evaluation path |

Do not rely on helper names alone: read the implementation and include `!entity.IsDeleted` and scope predicates explicitly where required.

### Dapper

`IDapperRepository<T>` and `DapperRepository<T>` exist, with `IDbConnectionFactory` / `SqlConnectionFactory`. Existing design comments describe Dapper as a read/report option; ordinary module CRUD should use EF Core unless a measured reporting/read use case needs Dapper. The current generic Dapper implementation has important caveats (tenant-filter parameter propagation, table-name pluralization, raw SQL identifier handling, and `ExecuteComplexQueryAsync`'s `GridReader` → `IDataReader` cast). Do not assume every generic method is production-safe without testing and reviewing its SQL and data-scope behavior. `ExecuteAsync` explicitly does not inject tenant filtering, so any use must provide a strict, correctly scoped WHERE clause.

## 8. Excel and CSV export: existing flow

The Excel service already exists and is registered in `Infrastructure/DependencyInjection.cs`:

```csharp
// Interface: Application/Common/Interfaces/IExcelExportService.cs
public interface IExcelExportService
{
    byte[] Export<T>(IEnumerable<T> items, string sheetName);
}

// Registration already exists in Infrastructure DependencyInjection:
services.AddScoped<IExcelExportService, ClosedXmlExportService>();
```

Implementation: `Infrastructure/SchoolERP.Infrastructure/Services/ClosedXmlExportService.cs` (ClosedXML).

- It creates a workbook and worksheet, uses the public properties of `T` as column headers, marks the header row bold/light gray, writes all items, adjusts column widths and returns `byte[]`.
- It treats a `DateTime` specially; other values use `ToString()`. It does not provide custom column-label metadata or complex nested-object mapping.
- **Prefer exporting a flat export DTO/light-response DTO**, not a database entity. That controls column order, names and exposure of sensitive/unnecessary properties.
- The usual flow is a MediatR export query -> scope-aware no-tracking EF query -> projection to export DTO -> `_excelExportService.Export(items, "SheetName")` -> `Result<byte[]>` -> controller returns a file.
- Use content type `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` and a safe timestamped filename.
- `CsvExportHelper.ToCsv()` is also present for CSV bytes when CSV, rather than Excel, is requested.

Reference implementation: `Application/SchoolERP.Application/Features/Academic/Queries/AcademicYear/ExportAcademicYear/ExportAcademicYearsQueryHandler.cs` and `API/SchoolERP.API/Controllers/AcademicYearsController.cs`. The export handler uses `IExcelExportService`; the controller performs a result check and returns `File(...)` on success.

## 9. How to add a new module — recommended checklist

Example name used below: `InventoryItem`. Replace it with the real module and follow the naming/namespace convention of the closest existing feature. Current source folder names are not fully uniform, so do not blindly rename the existing project structure.

### Step A — define ownership and entity

Decide:

- Is the entity platform-wide, tenant-only, or tenant + branch scoped?
- Does it need a `long` ID or a GUID ID? Follow similar entities rather than inventing a new ID convention.
- Is soft delete required? Most ordinary entities use `IsDeleted` / `DeletedAt`.
- Which relationships, uniqueness rules, and indexes are needed?

Example for a normal tenant + branch entity:

```csharp
using SchoolERP.Domain.Common;

namespace SchoolERP.Domain.Inventory.Entities;

public sealed class InventoryItem : BaseBranchEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
```

### Step B — add EF configuration

Example location: `Infrastructure/SchoolERP.Infrastructure/Persistence/Configurations/Inventory/InventoryItemConfiguration.cs`.

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolERP.Domain.Inventory.Entities;
using SchoolERP.Infrastructure.Persistence.Configurations.Common;

namespace SchoolERP.Infrastructure.Persistence.Configurations.Inventory;

public sealed class InventoryItemConfiguration : BaseBranchEntityConfiguration<InventoryItem>
{
    public override void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        base.Configure(builder);
        builder.ToTable("InventoryItems");
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        // Choose a unique index that matches business rules and soft-delete behavior.
        // Example only; add a filter if deleted records must not reserve the code.
        builder.HasIndex(x => new { x.TenantId, x.BranchId, x.Code });
    }
}
```

This config is discovered by `ApplyConfigurationsFromAssembly(...)`; do not add a manual configuration call unless the model registration is being intentionally changed. Add a `DbSet<InventoryItem>` to `AppDbContext` only if it improves local clarity/consistency; Application should still query through `IApplicationDbContext`.

### Step C — add Application DTOs and CQRS use cases

Use an area-specific DTO folder, like `Features/Academic/DTOs/` or the closest existing feature. Keep request DTOs and response DTOs separate; do not expose EF entities directly in API responses.

A typical set for CRUD + export is:

```text
Features/Inventory/
├── DTOs/
│   ├── CreateInventoryItemRequest.cs
│   ├── UpdateInventoryItemRequest.cs
│   ├── PatchInventoryItemRequest.cs        # only if PATCH is needed
│   ├── InventoryItemResponse.cs
│   └── InventoryItemExportRow.cs           # only if export shape differs
├── Commands/InventoryItem/
│   ├── CreateInventoryItem/                # command, handler, validator
│   ├── UpdateInventoryItem/                # command, handler, validator
│   ├── PatchInventoryItem/                 # optional
│   ├── DeleteInventoryItem/                # soft-delete handler
│   └── RestoreInventoryItem/               # optional
└── Queries/InventoryItem/
    ├── GetInventoryItems/                  # paged list
    ├── GetInventoryItemById/
    ├── GetInventoryItemsLight/              # only if dropdown data is needed
    └── ExportInventoryItems/                # only if export is required
```

Command record and handler signatures should follow the existing abstraction:

```csharp
public record CreateInventoryItemCommand(CreateInventoryItemRequest Request)
    : ICommand<InventoryItemResponse>;

public sealed class CreateInventoryItemCommandHandler
    : IRequestHandler<CreateInventoryItemCommand, Result<InventoryItemResponse>>
{
    // Inject IApplicationDbContext and the current-scope services needed by this feature.
    // Validate scope, duplicates and business rules before adding/mutating tracked entities.
    // Add using _dbContext.Set<InventoryItem>().Add(entity).
    // Map the response with Mapster and return Result.Success(response).
    // Do not manually SaveChangesAsync() in the normal command path.
}
```

Keep validator files beside the command. They are automatically found by `AddValidatorsFromAssembly(...)`. Reuse `EnsureUniqueAsync`, `EnsureAllUniqueAsync`, `GetEntityAsync`, and `PatchIfProvided` as appropriate.

### Step D — implement a safe list query

Use `PagedRequest` and return `Result<PagedResponse<InventoryItemResponse>>`. Typical steps:

1. Resolve and require tenant/branch context when the module requires it.
2. Start with `_dbContext.Set<InventoryItem>().AsNoTracking()`.
3. Add `!x.IsDeleted` explicitly; add explicit tenant/branch predicates where required by the actual entity model and current filter behavior.
4. Apply `SearchTerm` only to supported searchable fields.
5. Whitelist allowed sort fields rather than passing arbitrary `SortBy` input into dynamic SQL.
6. Validate/clamp `Page` and `Size` to positive values and a reasonable maximum.
7. Run `CountAsync`, then `Skip((Page - 1) * Size).Take(Size)`, map/project to response DTOs, return `PagedResponse<T>`.

For a single-record lookup/update/delete, use `GetEntityAsync` with an explicit predicate such as ID + relevant scope + `!IsDeleted`, then return `Error.NotFound(...)` via the helper if missing. Do not fetch by ID alone for branch-owned records.

### Step E — add controller and authorization

Use a controller similar in shape to `AcademicYearsController`, `StaffController`, or `MasterItemsController`, but **do not copy the current `AllowAnonymous` placeholders**. Use `HasPermissionAttribute` with the module's permission keys (for example `inventory_item.read`, `.create`, `.update`, `.delete`, `.export`) or the appropriate standard authorization required by the feature.

```csharp
[ApiController]
[Route("api/inventory-items")]
public sealed class InventoryItemsController : BaseApiController
{
    private readonly IMediator _mediator;

    public InventoryItemsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [HasPermission("inventory_item.read")]
    public async Task<IActionResult> Get([FromQuery] PagedRequest request)
        => HandleResult(await _mediator.Send(new GetInventoryItemsQuery(request)));
}
```

The sample shows the intended shape only. Add the remaining routes actually required by the feature, validate route ID vs body ID for PUT/PATCH (existing controllers do this), and use the correct permission names for the real RBAC data.

### Step F — wire database migration and verify

- The configuration class is found automatically through the EF assembly scan.
- Confirm the entity appears in the model and the right key/tenant/branch columns are generated.
- Inspect how the actual repository handles EF migrations; no migration files were included in the supplied archive. If migrations are used, create one with `AppDbContext` and the correct Infrastructure/ API startup projects, inspect the generated SQL, and apply it only to the intended database.
- Build the API, test create/list/detail/update/patch/delete/restore as applicable, verify branch and tenant isolation with two tenants/branches, test duplicate constraints and deleted-row behavior, and test export columns/content type.
- If repository tests exist outside the supplied archive, add or update tests for handler behavior, filters, permissions and mapping.

## 10. Existing feature references

Use these files as implementation examples; read the relevant entity, DTO, configuration, query/command handler and controller together before copying patterns:

- **CRUD + paging + detail + PATCH + soft delete + restore + bulk + Excel export:**
  - `API/SchoolERP.API/Controllers/AcademicYearsController.cs`
  - `Application/SchoolERP.Application/Features/Academic/Commands/AcademicYear/`
  - `Application/SchoolERP.Application/Features/Academic/Queries/AcademicYear/`
  - `Domain/SchoolERP.Domain/Academic/Entities/AcademicYear.cs`
  - `Infrastructure/SchoolERP.Infrastructure/Persistence/Configurations/Academic/AcademicYearConfiguration.cs`
- **Master data patterns:** `Features/Master/Commands/MasterCategory/`, `Features/Master/Commands/MasterItem/`, and matching controllers/configuration.
- **Student lifecycle patterns:** `Features/Student/Registration/`, `Features/Student/Enquiry/`, `Features/Student/Admission/`, plus `API/Controllers/Student/` and `Domain/SchoolERP.Domain/Student/`.
- **Staff IDs / staff documents:** `Features/Staff/`, `Domain/SchoolERP.Domain/Staff/`, `Infrastructure/Services/StaffIdGenerator.cs`.
- **Permission system:** `API/Authorization/`, `API/Controllers/PermissionsController.cs`, `API/Controllers/RolesController.cs`, `Infrastructure/Services/PermissionService.cs`, and `Domain/SchoolERP.Domain/Rbac/`.

## 11. Known implementation caveats to verify before relying on the current behavior

These observations are from the supplied source. They are recorded so a coding agent does not mistake current behavior for an intentional invariant. Address them when they affect the feature; do not perform a large unrelated refactor without being asked.

1. **Tenant and branch query filters need review.** In `AppDbContext.OnModelCreating`, the loop calls the unnamed `HasQueryFilter(...)` once for `IMustHaveTenant`, then again for `IMustHaveBranch`. With the current code shape, the second unnamed filter can replace the first for entities implementing both interfaces instead of combining both predicates. Verify and fix by composing predicates or using the supported named-filter pattern correctly for EF Core 10. Test with cross-tenant and cross-branch data.
2. **No global `IsDeleted` query filter is applied in `AppDbContext`.** `BaseEntityConfiguration` has a comment saying soft delete is handled globally, but the shown model loop applies only tenant/branch filters. Helpers also explicitly document that they do not auto-filter `IsDeleted`. New handlers must filter deleted rows intentionally until a tested global policy exists.
3. **Some entities are branch-only rather than tenant + branch.** The current `AcademicYear` extends `BranchEntity` and therefore lacks a direct `TenantId`. Do not reproduce this model for new tenant-owned data without ensuring the branch is validated against the active tenant and that query isolation is correct.
4. **Soft-delete uniqueness/indexes can conflict.** When adding a unique index, consider whether a soft-deleted row should continue reserving a code/name. Match the DB filtered index to the application's `IsDeleted`/`DeletedAt` rule; an application-level uniqueness check alone cannot override a conflicting unique database index.
5. **Many CRUD endpoints are currently marked `[AllowAnonymous]` with `TODO: [HasPermission(...)]`.** Treat this as incomplete authorization, not as the desired standard. Use proper permission attributes/authorization for new endpoints.
6. **Dapper generic methods need review before reuse.** In particular, the current `ExecuteComplexQueryAsync` attempts to cast Dapper's `GridReader` to `IDataReader`, generic table naming assumes `typeof(T).Name + "s"`, and tenant filter/parameter handling is inconsistent across methods. Prefer EF Core for normal writes and only use Dapper for reviewed, tested queries with explicit scope.
7. **Startup middleware has duplicate calls in the supplied source.** `Program.cs` currently calls `UseMultiTenant()`, `UseAuthentication()` and `UseAuthorization()` more than once, and contains console debug logging around authentication. Do not add further copies when implementing a module; review the pipeline separately if authentication/tenant behavior is part of the issue.
8. **`TenantDbContext` applies all configurations in the Infrastructure assembly.** Confirm whether this is intentional before modifying TenantDbContext/migrations because it can include non-tenant entity configurations in that model.
9. **Current page handling should not be copied blindly.** `PagedRequest` defaults to page 1/size 10 but doesn't validate the values itself. Ensure page/size are bounded in the handler or validator.
10. **Excel export is reflection-based and basic.** Pass a flat DTO with only intended properties and a valid worksheet name; do not expect the existing service to honor display-name attributes or special nested mappings.

## 12. Useful setup points

- Application registrations: `Application/SchoolERP.Application/DependencyInjection.cs` (MediatR behaviors and FluentValidation scan).
- Infrastructure registrations: `Infrastructure/SchoolERP.Infrastructure/DependencyInjection.cs` (DbContexts, `IApplicationDbContext`, identity, Finbuckle, Dapper, cache, ID generators, Excel export and AI services).
- API registrations: `API/SchoolERP.API/DependencyInjection.cs` (JWT, permission handler, health checks, API docs, telemetry).
- API middleware/startup: `API/SchoolERP.API/Program.cs`.
- Connection string and JWT/AI settings live in API `appsettings*.json`; **never paste real secrets or credentials into this README, code comments, prompts, or commits**. Use environment-specific secret configuration for real deployments.

---

### Compact context to paste into a new coding-agent chat

> You are modifying an existing SchoolERP backend targeting .NET 10. Respect the current four-layer structure: API controllers; Application CQRS handlers/DTOs/validators; Domain entities; Infrastructure EF Core/configurations/services. Use MediatR `ICommand<T>` for writes and `IQuery<T>` for reads, handlers returning `Result<T>`, FluentValidation validators, Mapster mapping, and `IApplicationDbContext.Set<TEntity>()`. `AppDbContext` auto-discovers Infrastructure `IEntityTypeConfiguration` classes and stamps tenant/branch/audit fields on added/saved entities. Choose the right Domain base entity (`BaseTenantEntity` or preferably `BaseBranchEntity` for tenant+branch data) and matching EF base configuration. Reuse `GetEntityAsync`, `EnsureUniqueAsync`/`EnsureAllUniqueAsync`, `PatchIfProvided`, `PagedRequest`/`PagedResponse`, current tenant/branch/user services, and existing ID generators. Excel is already implemented by `IExcelExportService` + `ClosedXmlExportService` and is registered; export flat DTOs to `byte[]` and return the Excel MIME type through controller `File(...)`. Do not add duplicate infrastructure or packages without a reason. Do not copy `[AllowAnonymous]` placeholders from CRUD controllers. Explicitly validate tenant and branch scope and `!IsDeleted` because current global filters do not reliably enforce all three; review the known query-filter and Dapper caveats above. Before coding, inspect the closest existing module end-to-end and keep the change minimal, production-safe, and consistent with the repository.
