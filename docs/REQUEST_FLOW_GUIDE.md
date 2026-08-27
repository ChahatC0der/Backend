# SchoolERP — Runtime Request Flow (Middlewares, Behaviors, End-to-End Trace)

> Companion to `ARCHITECTURE_AND_FLOW_GUIDE.md`. That file explains **structure/pattern**. This file explains **execution order** — what actually happens, in what sequence, when a request hits the API, using a real example (`POST /api/tenants`).

---

## 1. Startup Sequence (`Program.cs`)

```
1. WebApplication.CreateBuilder(args)
2. builder.Host.UseSerilog(...)                     -> logging provider wired first
3. builder.Services
	   .AddApplication()        -> MediatR + FluentValidation registered (Application/DependencyInjection.cs)
	   .AddInfrastructure(cfg)  -> EF Core, Identity, Finbuckle MultiTenant, Dapper, Services (Infrastructure/DependencyInjection.cs)
	   .AddApi(cfg)             -> JWT auth, RBAC policy provider, OpenTelemetry, HealthChecks, Controllers, Swagger/Scalar (API/DependencyInjection.cs)
4. builder.Services.AddHostedService<DatabaseHealthCheckService>()
5. var app = builder.Build();
```

### HTTP Middleware Pipeline (order = execution order, top to bottom)

```
app.UseSerilogRequestLogging()          // 1. Wraps every request in a structured log scope (method, path, status, elapsed ms)
app.UseMiddleware<GlobalExceptionHandlingMiddleware>()  // 2. try/catch around everything downstream
app.UseMultiTenant()                    // 3. Finbuckle resolves AppTenantInfo from Host or Header strategy, stores in IMultiTenantContextAccessor
   [Development only]
   app.UseSwagger() / UseSwaggerUI() / MapOpenApi() / MapScalarApiReference()
app.UseAuthentication()                 // 4. Validates JWT (if present), builds ClaimsPrincipal
app.UseAuthorization()                  // 5. Runs [Authorize]/[AllowAnonymous]/policy checks (PermissionHandler for [HasPermission])
app.MapHealthChecks("/health", ...)
app.MapControllers()                    // 6. Routes to controller action
app.Run()
```

**Why this order matters:**
- Exception middleware must wrap everything so any downstream exception (including from tenant resolution or MediatR) is caught and converted to a clean JSON error response.
- `UseMultiTenant()` runs **before** `UseAuthentication()`/`UseAuthorization()` so tenant context is available to any auth/permission logic and to `ICurrentTenantService` later in the pipeline.
- Authentication before Authorization (standard ASP.NET Core rule) — you must know *who* before checking *what they can do*.
- Controllers are mapped last — only requests that survive tenant resolution + auth reach action methods.

---

## 2. End-to-End Trace: `POST /api/tenants` (Create Tenant)

```
Client
  │  POST /api/tenants  { code, name, subdomain, contactEmail, plan }
  ▼
[1] Kestrel receives request
[2] SerilogRequestLogging middleware starts a log scope
[3] GlobalExceptionHandlingMiddleware.InvokeAsync -> try { await _next(context) }
[4] Finbuckle UseMultiTenant middleware
	  -> resolves tenant via Host/Header strategy from InMemoryStore
	  -> sets HttpContext multi-tenant context (AppTenantInfo)
[5] UseAuthentication -> [AllowAnonymous] on this endpoint, so no token required (skipped/ignored)
[6] UseAuthorization -> policy check passes ([AllowAnonymous])
[7] Routing -> TenantsController.Create(CreateTenantRequest request)
	  - [ApiController] triggers automatic model binding + ModelState validation
	  - If invalid model state -> API/DependencyInjection.cs custom
		InvalidModelStateResponseFactory returns 400 BadRequest
		{ success:false, error: "err1 | err2" }  (BEFORE controller code even runs)
[8] Controller action body:
	  return HandleResult(await Mediator.Send(new CreateTenantCommand(request)));
	  - Mediator resolved lazily via BaseApiController.Mediator property
[9] MediatR pipeline behaviors run IN THIS ORDER (registered in Application/DependencyInjection.cs):
	  a) LoggingBehavior<CreateTenantCommand, Result<TenantResponse>>
		   - logs "Handling CreateTenantCommand" before, "Handled" + elapsed after
	  b) ValidationBehavior<CreateTenantCommand, Result<TenantResponse>>
		   - resolves all IValidator<CreateTenantCommand> (here: CreateTenantValidator)
		   - runs FluentValidation rules (Code required/uppercase, Name required,
			 ContactEmail required+format, Plan in allowed set)
		   - IF INVALID -> short-circuits pipeline immediately, builds
			 Result<TenantResponse>.Failure(Error.Validation("msg1 | msg2"))
			 via reflection, RETURNS WITHOUT calling handler or hitting DB
	  c) PerformanceBehavior<CreateTenantCommand, Result<TenantResponse>>
		   - starts stopwatch, calls next(), logs warning if over threshold
	  d) TransactionBehavior<CreateTenantCommand, Result<TenantResponse>>
		   - request implements ICommandBase (it's a Command) -> transaction applies
		   - if no active transaction already:
				await _dbContext.BeginTransactionAsync()
				try {
					response = await next()          // <-- actual handler runs here
					await _dbContext.SaveChangesAsync()   // EF flush + AppDbContext overrides:
															//   - stamps TenantId on IMustHaveTenant entities
															//   - stamps CreatedAt/UpdatedAt on IAuditableTimestamps entities
					await _dbContext.CommitTransactionAsync()
				} catch { await _dbContext.RollbackTransactionAsync(); throw; }
[10] CreateTenantCommandHandler.Handle(...) executes (innermost, called by TransactionBehavior's next()):
	  - normalizes strings (subdomain lower+dash, code/name trim, email lower)
	  - builds an array of (Expression<Func<Tenant,bool>>, message) uniqueness checks
	  - await _dbContext.EnsureAllUniqueAsync<Tenant>(checks, ct)
		   -> runs .AnyAsync(predicate) per check against Tenants DbSet
		   -> NOTE: Tenant does NOT implement IMustHaveTenant, so NO tenant query filter
			  applies here (tenants are the root, not tenant-scoped)
		   -> if any duplicate found, returns combined Error.Conflict(...)
	  - if uniquenessError != null -> return uniquenessError (implicit Error -> Result<T> conversion)
									   -> TransactionBehavior still calls SaveChangesAsync()
										  (no-op, nothing added) then commits (empty tx)
	  - else: var tenant = request.Request.Adapt<Tenant>()  // Mapster maps DTO -> Entity
	  - _dbContext.Set<Tenant>().Add(tenant)   // tracked, NOT saved yet (SaveChanges happens in behavior)
	  - return Result.Success(tenant.Adapt<TenantResponse>())
[11] Control returns up through behaviors (Transaction commits -> Performance logs -> Validation passthrough -> Logging logs result)
[12] Back in Controller: HandleResult(result)
	  - result.IsSuccess == true -> Ok(result.Value, result.Message)
		-> returns 200 { success:true, data: TenantResponse }
	  - if it had failed -> Fail(result.Error)
		-> maps Error.Code to HTTP status:
			 NotFound    -> 404
			 Validation  -> 400
			 Unauthorized-> 401
			 Conflict    -> 409
			 (default)   -> 500
[13] If ANY unhandled exception occurred anywhere in steps 4-12,
	 GlobalExceptionHandlingMiddleware.HandleExceptionAsync catches it:
		ValidationException          -> 400 (FluentValidation's own exception type, rare
										  since ValidationBehavior normally intercepts first)
		UnauthorizedAccessException  -> 401
		KeyNotFoundException         -> 404
		anything else                -> 500 (message hidden unless Development)
[14] Serilog request logging middleware finishes the log scope with status code + duration
Client receives JSON response
```

---

## 3. Read Path Trace: `GET /api/tenants?page=1&size=10&searchTerm=abc` (differs from write path)

```
Routing -> TenantsController.GetAll([FromQuery] PagedRequest request)
Mediator.Send(new GetAllTenantsQuery(request))
MediatR behaviors:
  LoggingBehavior      -> logs
  ValidationBehavior   -> no validator registered for GetAllTenantsQuery -> passthrough
  PerformanceBehavior  -> timing
  TransactionBehavior  -> request is NOT ICommandBase (it's IQuery<T>) -> SKIPPED entirely,
						  calls next() directly, no BeginTransaction/SaveChanges/Commit
GetAllTenantsQueryHandler.Handle(...):
  - _dbContext.Set<Tenant>().Where(!IsDeleted)
  - optional SearchTerm -> .Where(Code/Name/Subdomain/Email.Contains(search))
  - SortBy/SortOrder switch -> OrderBy/OrderByDescending
  - CountAsync -> totalCount
  - Skip((page-1)*size).Take(size).ToListAsync()
  - Adapt to IEnumerable<TenantResponse>
  - return Result.Success(PagedResponse<TenantResponse>{...})
Controller -> HandleResult -> 200 { success:true, data: { data:[...], page, size, totalCount, totalPages } }
```

Key difference from write path: **no transaction, no SaveChanges** — pure read, EF change tracking still applies (default) but nothing is persisted.

---

## 4. Multi-Tenant Data Isolation — When It Kicks In

For entities implementing `IMustHaveTenant`/`IMustHaveBranch` (i.e., NOT Tenant/Branch themselves, but future business entities like Student, Fee, etc.):

```
Any EF query (e.g., _dbContext.Set<Student>().ToListAsync())
   ▼
EF Core applies the global HasQueryFilter registered in AppDbContext.OnModelCreating:
   WHERE TenantId = @CurrentTenantId
   ▼
@CurrentTenantId is resolved LAZILY at query execution time via:
   ICurrentTenantService.GetTenantId()
	   -> IMultiTenantContextAccessor<AppTenantInfo>.MultiTenantContext.TenantInfo.Id
	   -> this was populated back in step [4] of the middleware pipeline (UseMultiTenant())
```

On **write** (`SaveChangesAsync` override in `AppDbContext`):
```
foreach entry in ChangeTracker.Entries<IMustHaveTenant>()
	if entry.State == Added:
		entry.Entity.TenantId = CurrentTenantId    // auto-stamped, handler never sets this manually

foreach entry in ChangeTracker.Entries<IAuditableTimestamps>()
	if Added:    CreatedAt = UpdatedAt = UtcNow
	if Modified: UpdatedAt = UtcNow
```

This is why handlers (Create/Update/Patch) never set `TenantId`, `CreatedAt`, or `UpdatedAt` manually — Mapster profiles explicitly `.Ignore()` these fields, and `AppDbContext` fills them in automatically at `SaveChangesAsync` time (which itself is only ever called from `TransactionBehavior`).

---

## 5. Validation Failure Trace (short-circuit example)

```
POST /api/tenants  { code: "", name: "", contactEmail: "not-an-email" }

1. Model binding succeeds (all fields are strings, nothing throws)
2. [ApiController] built-in ModelState validation: passes (no [Required] attributes on record ctor params
   unless explicitly added) -- so it reaches the controller action
3. Controller -> Mediator.Send(new CreateTenantCommand(request))
4. LoggingBehavior -> logs start
5. ValidationBehavior:
	 - CreateTenantValidator.Validate(command) runs FluentValidation rules
	 - failures: "Tenant code is required.", "Tenant name is required.", "Invalid email format."
	 - error = Error.Validation("Tenant code is required. | Tenant name is required. | Invalid email format.")
	 - detects TResponse == Result<TenantResponse> (generic), builds via reflection:
		 Result.Failure<TenantResponse>(error)
	 - returns immediately -- PerformanceBehavior, TransactionBehavior, and the actual
	   CreateTenantCommandHandler NEVER EXECUTE. No DB call is made at all.
6. Controller: HandleResult(result) -> result.IsFailure -> Fail(error)
	 -> error.Code == "Validation" -> BadRequest(400)
	 -> { success:false, error: "Tenant code is required. | Tenant name is required. | Invalid email format." }
```

---

## 6. Conflict Failure Trace (business-rule, not FluentValidation)

```
POST /api/tenants  { code:"DAV", name:"DAV School", subdomain:"dav", contactEmail:"x@dav.com", plan:"basic" }
(assume code "DAV" already exists in DB)

1-4. same as above, FluentValidation passes (all fields technically valid)
5. ValidationBehavior passes through (no rule violations)
6. PerformanceBehavior -> starts timer -> calls next()
7. TransactionBehavior -> BeginTransactionAsync() -> calls next()
8. CreateTenantCommandHandler.Handle:
	 - EnsureAllUniqueAsync finds Code "DAV" already exists
	 - returns Error.Conflict("A tenant with the code 'DAV' already exists.")
9. TransactionBehavior: response returned by next() is a Result.Failure (not an exception)
	 -> still calls SaveChangesAsync() (no-op, nothing tracked as Added)
	 -> still commits the (empty) transaction -> no rollback needed since no exception thrown
10. Controller: HandleResult -> error.Code == "Conflict" -> 409
	 -> { success:false, error: "A tenant with the code 'DAV' already exists." }
```

**Important nuance:** Business-rule failures (`Result.Failure`) do NOT trigger a transaction rollback — because they don't throw exceptions. Only unhandled exceptions inside the handler (e.g., a DB connectivity error) trigger `RollbackTransactionAsync()` in the `catch` block of `TransactionBehavior`.

---

## 7. Export Flow (File Download) Trace: `GET /api/tenants/export`

```
Controller.Export():
  result = await Mediator.Send(new ExportTenantsQuery())
  (goes through Logging -> Validation(none) -> Performance -> Transaction(SKIPPED, it's a Query) -> Handler)
  ExportTenantsQueryHandler:
	 - queries Tenants (not deleted)
	 - projects to rows
	 - calls IExcelExportService (ClosedXmlExportService) to build an .xlsx byte[]
	 - returns Result.Success(byte[])
  Controller:
	 if result.IsFailure -> HandleResult(result)   // normal JSON error path
	 else -> return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName)
			 // bypasses HandleResult entirely for the success case -- this is the ONE exception
			 // to the "always use HandleResult" rule, specific to binary file responses
```

---

## 8. Summary Diagram

```
HTTP Request
  └─ Serilog logging scope
	  └─ GlobalExceptionHandlingMiddleware (try)
		  └─ Finbuckle UseMultiTenant (resolves AppTenantInfo)
			  └─ UseAuthentication (JWT, if any)
				  └─ UseAuthorization ([AllowAnonymous] / [HasPermission] policies)
					  └─ Routing → Controller Action (inherits BaseApiController)
						  └─ Mediator.Send(Command/Query)
							  └─ LoggingBehavior
								  └─ ValidationBehavior (FluentValidation; short-circuits on failure)
									  └─ PerformanceBehavior (timing)
										  └─ TransactionBehavior (Commands only: Begin→next()→SaveChanges→Commit/Rollback)
											  └─ Concrete Handler
												  ├─ Uniqueness checks (EnsureAllUniqueAsync)
												  ├─ Fetch-or-NotFound (GetEntityByIdAsync)
												  ├─ Mapster DTO↔Entity mapping
												  ├─ EF Core DbSet Add/Update/Remove (tracked only)
												  └─ return Result<T>.Success(...) or Error
		  (catch) → JSON error response (500/etc. for anything unhandled above)
	  Controller.HandleResult(result) → maps Result<T> to 200/400/401/404/409/500 JSON
  Response sent, Serilog logs final status + duration
```

---

*Pairs with `ARCHITECTURE_AND_FLOW_GUIDE.md`. That document = "where things live and how to build a new module." This document = "what actually executes, in what order, for a real request." Keep both in sync when the pipeline (behaviors/middlewares) changes.*
