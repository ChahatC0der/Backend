# SchoolERP — Architecture & API Flow Guide

> Reference document for building new features (Modules) using the exact same pattern already implemented and proven in the **Tenants** and **Branches** modules.
> Master and RBAC modules are still under active development — **do not copy patterns from them**. Use Tenants/Branches as the golden reference.

---

## 1. Tech Stack & Package Versions

**Target Framework:** `net10.0` (all projects), `Nullable` + `ImplicitUsings` enabled everywhere.

### src/API/SchoolERP.API (Presentation Layer)
| Package | Version |
|---|---|
| AspNetCore.HealthChecks.Hangfire | 9.0.0 |
| AspNetCore.HealthChecks.SqlServer | 9.0.0 |
| AspNetCore.HealthChecks.UI | 9.0.0 |
| AspNetCore.HealthChecks.UI.Client | 9.0.0 |
| Finbuckle.MultiTenant | 10.1.2 |
| Finbuckle.MultiTenant.AspNetCore | 10.1.1 |
| FluentValidation.DependencyInjectionExtensions | 12.1.1 |
| Hangfire.AspNetCore | 1.8.24 |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.10 |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 10.0.10 |
| Microsoft.AspNetCore.Identity.UI | 10.0.10 |
| Microsoft.AspNetCore.OpenApi | 10.0.8 |
| Microsoft.EntityFrameworkCore.Design | 10.0.10 |
| Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore | 10.0.10 |
| OpenIddict.AspNetCore / EntityFrameworkCore | 7.6.0 |
| OpenTelemetry.* (OTLP, Hosting, AspNetCore, SqlClient) | 1.17.0 |
| Scalar.AspNetCore | 2.16.17 |
| Serilog.AspNetCore | 10.0.0 |
| Swashbuckle.AspNetCore | 10.2.3 |

### src/Application/SchoolERP.Application (Use-Cases / CQRS Layer)
| Package | Version |
|---|---|
| FluentValidation | 12.1.1 |
| FluentValidation.DependencyInjectionExtensions | 12.1.1 |
| Mapster | 10.0.11 |
| MediatR | 12.4.1 |
| Microsoft.EntityFrameworkCore | 10.0.10 |

### src/Infrastructure/SchoolERP.Infrastructure (Data/External Concerns)
| Package | Version |
|---|---|
| AWSSDK.S3 | 4.0.101.5 |
| ClosedXML | 0.105.1 |
| Dapper | 2.1.79 |
| Finbuckle.MultiTenant / .EntityFrameworkCore / .AspNetCore | 10.1.2 / 10.0.3 / 10.1.1 |
| Hangfire.Core / Hangfire.SqlServer | 1.8.24 |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 10.0.10 |
| Microsoft.Data.SqlClient | 7.0.2 |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.10 |
| OpenIddict.* | 7.6.0 |
| OpenTelemetry | 1.17.0 |
| Serilog | 4.4.0 |

### src/Domain/SchoolERP.Domain & SchoolERP.Domain.Shared
No third-party packages — pure POCO/entities and shared kernel (Result pattern, Permission name constants).

> **Rule:** When adding a new module, do NOT introduce new packages unless the capability truly doesn't exist. Reuse Mapster, MediatR, FluentValidation, EF Core, Dapper, ClosedXML (exports) that are already wired in.

---

## 2. Solution / Folder Structure (Clean Architecture, Onion style)

```
src/
  Domain/
	SchoolERP.Domain.Shared/        <- Cross-cutting: Result<T>, Error, PermissionNames (no EF, no deps)
	SchoolERP.Domain/                <- Entities, base classes, marker interfaces (no deps besides Shared)
	  Common/                        <- BaseEntity.cs, BaseAuditableEntity.cs (entity base hierarchy)
	  Tenants/Entities/               <- Tenant.cs, Branch.cs
	  Rbac/Entities/                  <- (in progress, ignore for now)
  Application/
	SchoolERP.Application/
	  Common/
		Abstractions/ICommand.cs      <- ICommand<T>, IQuery<T>, ICommandBase marker
		Behaviors/                    <- MediatR pipeline behaviors (Logging, Validation, Performance, Transaction)
		DTOs/                         <- PagedRequest, PagedResponse (generic, reused by all modules)
		Extensions/                   <- UniquenessCheckExtensions, PatchHelperExtensions, EntityExistsExtensions, CsvExportHelper
		Interfaces/                   <- IApplicationDbContext, ICurrentTenantService, ICacheService, IDapperRepository, IExcelExportService
	  Features/
		Tenants/                       <- 🌟 GOLDEN REFERENCE MODULE
		  Commands/<UseCase>/<UseCase>Command.cs + Handler.cs + Validator.cs
		  Queries/<UseCase>/<UseCase>Query.cs + Handler.cs
		  DTOs/TenantDtos.cs           <- All request/response records for the module in ONE file
		  Mappings/ (if any)           <- Mapster IRegister profiles
		Branches/                      <- 🌟 GOLDEN REFERENCE MODULE (Tenant-scoped variant)
		  same structure as Tenants + Mappings/BranchMappingProfile.cs
		Masters/, Rbac/                <- IN PROGRESS — do not use as pattern reference yet
	  DependencyInjection.cs           <- registers MediatR + FluentValidation for this assembly
  Infrastructure/
	SchoolERP.Infrastructure/
	  Data/DapperConnectionFactory.cs, DapperRepository.cs
	  Identity/ApplicationUser.cs
	  MultiTenancy/AppTenantInfo.cs
	  Persistence/
		AppDbContext.cs                <- IdentityDbContext + IApplicationDbContext implementation
		Configurations/                <- IEntityTypeConfiguration<T> per entity, using Base(Guid)EntityConfiguration
	  Services/                        <- ClosedXmlExportService, CurrentTenantService, MemoryCacheService, DatabaseHealthCheckService
	  DependencyInjection.cs           <- EF Core, Identity, Finbuckle MultiTenant, Dapper, Services registration
  API/
	SchoolERP.API/
	  Authorization/                   <- HasPermissionAttribute, PermissionHandler, PermissionPolicyProvider, PermissionRequirement (RBAC - in progress)
	  Controllers/
		BaseApiController.cs           <- Ok<T>(), Fail(Error), HandleResult<T>(Result<T>) — ALL controllers must inherit this
		TenantsController.cs           <- 🌟 GOLDEN REFERENCE
		BranchesController.cs          <- 🌟 GOLDEN REFERENCE
		MastersController.cs, AuthController.cs <- in progress
	  Filters/ValidateModelStateFilter.cs
	  Middleware/GlobalExceptionHandlingMiddleware.cs
	  DependencyInjection.cs           <- JWT auth, RBAC policy provider, OpenTelemetry, HealthChecks, Controllers, Swagger/Scalar
	  Program.cs                       <- composition root or app pipeline
tests/
  SchoolERP.UnitTests/
  SchoolERP.IntegrationTests/
```

**Naming convention:** `Features/<ModuleName>/{Commands|Queries}/<UseCaseName>/<UseCaseName>{Command|Query}.cs`, `...Handler.cs`, `...Validator.cs` (only for Commands). DTOs for the whole module live in one file: `Features/<ModuleName>/DTOs/<ModuleName>Dtos.cs`.

---

## 3. Layered Responsibilities

- **Domain.Shared** — `Result`/`Result<T>`/`Error` (functional result pattern, no exceptions for expected failures), `PermissionNames` constants.
- **Domain** — Entities only. No logic beyond simple invariants. Entities inherit from base classes in `Common/BaseEntity.cs`:
  - `IdEntity` (long Id) / `GuidEntity` (Guid Id, auto `Guid.NewGuid()`)
  - `BaseEntity : IdEntity, IAuditableTimestamps` → `CreatedAt, UpdatedAt, IsDeleted, DeletedAt`
  - `BaseAuditableEntity : BaseEntity` → adds `CreatedBy`/`UpdatedBy` (Guid?)
  - `GuidAuditableEntity : GuidEntity, IAuditableTimestamps` → **used ONLY by Tenant and Branch** (their Id is a Guid, not long)
  - `IMustHaveTenant` (Guid TenantId) / `IMustHaveBranch : IMustHaveTenant` (+ Guid BranchId) — marker interfaces that drive **automatic EF Core global query filters** and **automatic TenantId stamping** in `AppDbContext`.
  - `TenantEntity`, `TenantAuditableEntity`, `BranchEntity`, `BranchAuditableEntity` — abstract bases for regular (long Id) tenant/branch-scoped entities (e.g., future Student, Fee, etc.)

  **Rule for new modules:** If the entity is tenant-scoped business data (most future modules), inherit `TenantAuditableEntity` or `BranchAuditableEntity`. Only `Tenant`/`Branch` themselves use `GuidAuditableEntity` directly since they don't have a parent tenant filter.

- **Application** — CQRS with MediatR. Business logic, validation rules, orchestration. Talks to `IApplicationDbContext` (never the concrete `AppDbContext`) and other abstraction interfaces (`ICurrentTenantService`, `ICacheService`, `IDapperRepository`, `IExcelExportService`). No EF/Infra package leaks besides `Microsoft.EntityFrameworkCore` (for `IQueryable`/`DbSet` extension methods like `.AnyAsync/.ToListAsync`).
- **Infrastructure** — EF Core `AppDbContext` (implements `IApplicationDbContext`), entity configurations, Identity, Finbuckle MultiTenant, Dapper, export/cache services, health checks.
- **API** — Thin controllers, MediatR dispatch only, `BaseApiController.HandleResult()` for every response, global exception middleware, DI composition per layer via extension methods (`AddApplication()`, `AddInfrastructure()`, `AddApi()`) called from `Program.cs`.

---

## 4. The CQRS Request Pipeline (MediatR Behaviors)

Registered in `Application/DependencyInjection.cs`, **order matters**:
1. `LoggingBehavior<,>` — logs request/response
2. `ValidationBehavior<,>` — runs all FluentValidation validators for the request; short-circuits with `Result.Failure(Error.Validation(...))` on failure (uses reflection to build the correct generic `Result<T>`)
3. `PerformanceBehavior<,>` — logs slow requests
4. `TransactionBehavior<,>` — **only for `ICommandBase`** (i.e., Commands, not Queries): begins EF transaction, calls next(), `SaveChangesAsync()`, commits; rolls back on exception. Skips if a transaction is already active (re-entrancy safe).

`ICommand<TResponse> : IRequest<Result<TResponse>>, ICommandBase` and `IQuery<TResponse> : IRequest<Result<TResponse>>` — every handler returns `Result<T>`, never throws for business-rule failures.

---

## 5. Golden-Path Recipe: Adding a New CRUD Module (follow Tenants/Branches exactly)

### Step 1 — Domain Entity
`src/Domain/SchoolERP.Domain/<Module>/Entities/<Entity>.cs`
- Inherit `TenantAuditableEntity` (long Id, tenant-scoped) or `BranchAuditableEntity` (tenant+branch scoped) for normal business entities. Only use `GuidAuditableEntity` if it's a root-like entity similar to Tenant/Branch.
- Plain properties with sensible defaults set in constructor if needed (see `Tenant()` ctor pattern: `Status = "active"`, JSON columns default to `"{}"`).

### Step 2 — EF Configuration
`src/Infrastructure/SchoolERP.Infrastructure/Persistence/Configurations/<Entity>Configuration.cs`
- Inherit `BaseGuidEntityConfiguration<TEntity>` (for Guid-keyed) — sets `HasKey`, `ValueGeneratedNever()` (App sets Id in C#, not DB), `IsDeleted` default false, required timestamps.
- For long-Id entities, an equivalent `BaseEntityConfiguration<TEntity>` exists — use it.
- Add `builder.ToTable(...)`, column lengths, defaults, unique indexes (`HasIndex(...).IsUnique()`), all in the `Configure()` override, calling `base.Configure(builder)` first.
- No manual registration needed — `AppDbContext.OnModelCreating` calls `modelBuilder.ApplyConfigurationsFromAssembly(...)` automatically.

### Step 3 — DbSet
Add `public DbSet<TEntity> <Entities> => Set<TEntity>();` to `AppDbContext`. Global query filters for `IMustHaveTenant`/`IMustHaveBranch` are applied automatically via reflection in `OnModelCreating` — **no extra code needed** as long as the entity implements the marker interface (inherited via base class).

### Step 4 — DTOs (single file)
`Application/Features/<Module>/DTOs/<Module>Dtos.cs` — use C# `record` types:
- `Create<Entity>Request`
- `Update<Entity>Request` (full update)
- `Patch<Entity>Request` (all nullable — partial update)
- `Bulk<Entity>Request` variants (`List<Guid> Ids` + nullable fields) for BulkUpdate/BulkPatch
- `<Entity>Response` (full projection)
- `<Entity>LightResponse` (Id + minimal fields, for dropdowns)
- Shared `BulkDeleteRequest(List<Guid> Ids)` already exists in Common — reuse it, don't redefine per module.

### Step 5 — Mapster Mapping Profile
`Application/Features/<Module>/Mappings/<Module>MappingProfile.cs` implementing `IRegister`:
- Map `Create...Request → Entity`: trim/normalize strings, `.Ignore()` server-controlled fields (`Id`, `TenantId`, `CreatedAt`, `UpdatedAt`, `IsDeleted`, `DeletedAt`, navigation props).
- Map `Update...Request → Entity` similarly (still ignore audit/id fields).
- Map `Patch...Request → Entity` — null-safe partial mapping (see `PatchHelperExtensions.PatchIfProvided` for manual patch logic used in handlers, or Mapster conditional mapping).
- Register profile is auto-discovered if Mapster global config scans assembly (verify existing bootstrap — Branches profile is auto-loaded already).

### Step 6 — Commands (Write side) — one folder per use case
For each of `CreateBranch/CreateTenant`-style use cases, replicate this trio:
- `<UseCase>Command.cs`: `public record CreateXCommand(CreateXRequest Request) : ICommand<XResponse>;` (or `(Guid Id, ...)` for update/patch/delete)
- `<UseCase>CommandHandler.cs`: implements `IRequestHandler<XCommand, Result<XResponse>>`, constructor-injects `IApplicationDbContext`, uses:
  - `UniquenessCheckExtensions.EnsureAllUniqueAsync(...)` for multi-field duplicate checks → returns `Error.Conflict` combined message
  - `EntityExistsExtensions`/`GetEntityByIdAsync<T>` helper for fetch-or-NotFound pattern (see `PatchTenantCommandHandler`)
  - Mapster `.Adapt<Entity>()` for Create; manual/Mapster patch for Update/Patch
  - `_dbContext.Set<Entity>().Add(...)` — no explicit `SaveChangesAsync()` call needed (handled by `TransactionBehavior`)
  - Returns `Result.Success(response.Adapt<XResponse>())` or an `Error` (implicit conversion to `Result<T>`)
- `<UseCase>Validator.cs` (FluentValidation, Commands only — not Queries): `AbstractValidator<XCommand>`, rules on `x.Request.<Field>`.

Standard command set per module: `CreateX`, `UpdateX` (full), `PatchX` (partial), `DeleteX` (soft delete), `RestoreX`, `BulkUpdateX`, `BulkPatchX`, `BulkDeleteX`.

### Step 7 — Queries (Read side)
- `GetXById`, `GetAllX` (paged, using shared `PagedRequest`/`PagedResponse<T>`), `GetAllXLight` (dropdown), `ExportX` (CSV/XLSX via `ClosedXmlExportService`/`IExcelExportService`, returns `byte[]`).
- `GetAllXQueryHandler` pattern: `_dbContext.Set<Entity>().Where(!IsDeleted)`, optional `SearchTerm` filter (`.Contains` on lowercased fields), switch-based `SortBy`/`SortOrder`, `CountAsync`, `Skip/Take`, `.Adapt<IEnumerable<XResponse>>()`.
- Queries have NO validator typically (unless input needs validation), and are NOT wrapped by `TransactionBehavior` (only `ICommandBase` triggers it) — pure reads.

### Step 8 — Controller
`API/Controllers/<Module>sController.cs` : `BaseApiController`, `[ApiController] [Route("api/[controller]")]`.
- Every action: `=> HandleResult(await Mediator.Send(new XCommand(...)));` one-liner style.
- HTTP verb mapping: `POST` create, `GET` list (paged) + `GET "all"` (light) + `GET "{id:guid}"`, `PUT "{id}"` full update, `PUT "bulk"` bulk update, `PATCH "{id}"` partial, `PATCH "bulk"` bulk patch, `DELETE "{id:guid}"` soft delete, `POST "{id:guid}/restore"`, `DELETE "bulk"` bulk delete, `GET "export"` file download (checks `result.IsFailure` first, then returns `File(bytes, mimeType, fileName)`).
- Currently endpoints use `[AllowAnonymous]` with `// TODO: [HasPermission("...")]` comments — RBAC wiring is in progress; keep this pattern (don't force permission attributes yet) for new modules unless told otherwise.

---

## 6. Cross-Cutting Helpers to Reuse (do not reinvent)

| Helper | Location | Purpose |
|---|---|---|
| `Result` / `Result<T>` / `Error` | `Domain.Shared/Results/Result.cs` | Functional success/failure with `Error.NotFound/Conflict/Validation/Unauthorized` factories |
| `BaseApiController` | `API/Controllers/BaseApiController.cs` | `Ok<T>`, `Fail(Error)`, `HandleResult<T>(Result<T>)` — maps Error.Code to HTTP status (NotFound→404, Validation→400, Unauthorized→401, Conflict→409, else 500) |
| `UniquenessCheckExtensions` | `Application/Common/Extensions` | `EnsureUniqueAsync` (single) / `EnsureAllUniqueAsync` (batch, combines messages with `" | "`) |
| `PatchHelperExtensions.PatchIfProvided` | `Application/Common/Extensions` | Apply setter only if string value is non-empty/whitespace |
| `EntityExistsExtensions` | `Application/Common/Extensions` | `GetEntityByIdAsync<T>` fetch-or-`NotFound` Result helper |
| `PagedRequest` / `PagedResponse<T>` | `Application/Common/DTOs` | Standard pagination contracts (Page, Size, SortBy, SortOrder, SearchTerm / Data, TotalCount, TotalPages) |
| `CsvExportHelper` / `IExcelExportService` (`ClosedXmlExportService`) | Application/Infrastructure | XLSX export for `ExportX` queries |
| `GlobalExceptionHandlingMiddleware` | `API/Middleware` | Catches unhandled exceptions → maps `ValidationException`→400, `UnauthorizedAccessException`→401, `KeyNotFoundException`→404, else 500 (message hidden in Production) |
| `ICurrentTenantService` (`CurrentTenantService`) | Infrastructure/Services | Resolves current tenant Guid/Name/Identifier via Finbuckle `IMultiTenantContextAccessor<AppTenantInfo>` |

---

## 7. Multi-Tenancy Mechanics (Finbuckle)

- `AddInfrastructure()` registers `AddMultiTenant<AppTenantInfo>().WithHostStrategy().WithHeaderStrategy().WithInMemoryStore(...)` (currently seeded in-memory tenants — replace with DB store when ready, but don't change pattern without discussion).
- `app.UseMultiTenant()` is placed in `Program.cs` right after exception middleware, before Swagger/Auth.
- `AppDbContext` resolves `CurrentTenantId` via `ICurrentTenantService.GetTenantId()`.
- `OnModelCreating` walks all entity types; for any implementing `IMustHaveBranch` or `IMustHaveTenant`, it auto-applies a `HasQueryFilter` on `TenantId == CurrentTenantId` — **new tenant-scoped entities get isolation for free**, just implement the marker interface (via base class).
- `SaveChangesAsync` override auto-stamps `TenantId` on Added entries implementing `IMustHaveTenant`, and `CreatedAt`/`UpdatedAt` on `IAuditableTimestamps` entries (Added → both set; Modified → `UpdatedAt` only).

---

## 8. What to Ignore for Now

- `Features/Masters/*` and `Features/Rbac/*` (Application layer) — under active development, do not treat as reference pattern.
- `API/Authorization/*` (`HasPermissionAttribute`, `PermissionHandler`, etc.) and `MastersController`, `AuthController` — RBAC/permission enforcement still WIP; new modules should keep `[AllowAnonymous]` + `// TODO: [HasPermission(...)]` comments like Tenants/Branches do, until RBAC is finalized.

---

## 9. Quick Checklist for a New Module (e.g., "Students")

1. [ ] Entity in `Domain/<Module>/Entities/` inheriting the correct base (`TenantAuditableEntity`/`BranchAuditableEntity`)
2. [ ] EF Configuration in `Infrastructure/Persistence/Configurations/` inheriting `Base(Guid)EntityConfiguration<T>`
3. [ ] `DbSet<T>` added to `AppDbContext`
4. [ ] DTOs file: Create/Update/Patch/Bulk*/Response/LightResponse records
5. [ ] Mapster `IRegister` profile: Create/Update/Patch mappings, ignoring server fields
6. [ ] Commands: Create, Update, Patch, Delete, Restore, BulkUpdate, BulkPatch, BulkDelete (Command + Handler + Validator where applicable)
7. [ ] Queries: GetById, GetAll (paged), GetAllLight, Export
8. [ ] Controller inheriting `BaseApiController`, one-liner actions via `HandleResult(await Mediator.Send(...))`
9. [ ] Build (`dotnet build`) and verify no DI/mapping errors
10. [ ] Add/verify unit tests in `tests/SchoolERP.UnitTests` following existing test conventions (if present for Tenants/Branches)

---

*Generated as a reference guide from the current state of the SchoolERP codebase. Keep this file updated whenever the Tenants/Branches pattern evolves.*
