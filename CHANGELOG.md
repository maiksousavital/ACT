# ACT — Implementation Changelog

All changes made during the SaaS implementation plan, grouped by phase.

---

## Phase 1 — Multi-Tenancy Foundation

### 1.1 Add CompanyId to entities *(completed 2026-05-06)*

**Domain — Entities modified:**
- `Client.cs` — added `int CompanyId` + `Company` navigation property
- `TreatmentType.cs` — added `int CompanyId` + `Company` navigation property
- `Treatment.cs` — added `int CompanyId` + `Company` navigation property
- `Company.cs` — added `Clients`, `TreatmentTypes`, `Treatments` navigation collections

**Infrastructure — DbContext:**
- `AppDbContext.cs` — configured FK relationships for Client→Company, TreatmentType→Company, Treatment→Company (all with `OnDelete(Cascade)`)

**Migration:**
- `20260506053151_AddCompanyIdToEntities` — adds nullable `CompanyId` columns + FK indexes to Clients, Treatments, TreatmentTypes

---

### 1.2 Backfill & make required *(completed 2026-05-06)*

**Domain — Entities modified:**
- `Client.cs` — changed `int? CompanyId` → `int CompanyId` (required)
- `TreatmentType.cs` — changed `int? CompanyId` → `int CompanyId` (required)
- `Treatment.cs` — changed `int? CompanyId` → `int CompanyId` (required)

**Infrastructure — DbContext:**
- `AppDbContext.cs` — changed FK delete behavior from `SetNull` → `Cascade`
- `AppDbContext.cs` — added seed data for default Company (Id=1)
- `AppDbContext.cs` — updated TreatmentType seed data to include `CompanyId = 1`

**Migration:**
- `20260506053605_BackfillCompanyIdAndMakeRequired` — backfills existing rows with `CompanyId = 1` via SQL, then makes columns non-nullable. Drops and recreates FKs as Cascade.

---

### 1.3 Update repositories & services *(completed 2026-05-06)*

**Domain — Interfaces modified:**
- `IClientRepository.cs` — added `companyId` param to `GetAllAsync`, `GetPagedAsync`
- `ITreatmentRepository.cs` — added `companyId` param to `GetDueAsync`, `GetTodayAsync`, `GetPagedAsync`
- `ITreatmentTypeRepository.cs` — added `companyId` param to `GetAllActiveAsync`, `GetPagedAsync`

**Infrastructure — Repositories modified:**
- `ClientRepository.cs` — filters by `CompanyId` in `GetAllAsync`, `GetPagedAsync`; fixed async warning in `UpdateAsync`
- `TreatmentRepository.cs` — filters by `CompanyId` in `GetDueAsync`, `GetTodayAsync`, `GetPagedAsync`
- `TreatmentTypeRepository.cs` — filters by `CompanyId` in `GetAllActiveAsync`, `GetPagedAsync`

**Infrastructure — Services modified:**
- `FollowUpNotificationWorker.cs` — changed from `ITreatmentRepository.GetDueAsync()` to `AppDbContext` direct query (cross-company background worker)

**Application — Service interfaces modified:**
- `IClientService.cs` — added `companyId` to `GetAllAsync`, `CreateAsync`, `GetPagedAsync`
- `ITreatmentService.cs` — added `companyId` to `GetDueAsync`, `GetTodayAsync`, `CreateAsync`, `GetPagedAsync`; added `CompleteFollowUpAsync` to interface
- `ITreatmentTypeService.cs` — added `companyId` to `GetAllActiveAsync`, `CreateAsync`, `GetPagedAsync`

**Application — Services modified:**
- `ClientService.cs` — passes `companyId` through; sets `CompanyId` on new entities
- `TreatmentService.cs` — passes `companyId` through; sets `CompanyId` on new treatments and follow-ups
- `TreatmentTypeService.cs` — passes `companyId` through; sets `CompanyId` on new treatment types

**API — Controllers modified:**
- `ClientController.cs` — added `CompanyId` property (reads from `HttpContext.Items`, defaults to 1); passes to service calls
- `TreatmentController.cs` — same pattern; cleaned up old commented-out code
- `TreatmentTypeController.cs` — same pattern
- `FollowUpsController.cs` — changed from concrete `TreatmentService` to `ITreatmentService` interface; added `CompanyId` property

**API — Program.cs:**
- Removed redundant `AddScoped<TreatmentService>()` (concrete type)
- Removed duplicate `AddScoped<ITreatmentService, TreatmentService>()`

---

### 1.4 Temporary CompanyId resolution — SKIPPED

> Controllers already default `CompanyId = 1`. Phase 3 (JWT auth) will replace with `User.Claims` extraction. No throwaway middleware needed.

---

## Phase 2 — Company CRUD & Onboarding *(completed 2026-05-06)*

**Domain — New files:**
- `Interfaces/ICompanyRepository.cs` — `GetAllAsync`, `GetByIdAsync`, `AddAsync`, `UpdateAsync`, `SaveChangesAsync`, `GetPagedAsync`

**Application — New files:**
- `Dtos/CompanyDto.cs` — response DTO (Id, Name, ContactEmail, Phone, Address)
- `Dtos/CreateCompanyRequest.cs` — create request DTO
- `Dtos/UpdateCompanyRequest.cs` — update request DTO
- `Services/Interfaces/ICompanyService.cs` — `GetAllAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `GetPagedAsync`
- `Services/CompanyService.cs` — implementation with `ToDto` mapping

**Infrastructure — New files:**
- `Repositories/CompanyRepository.cs` — EF Core implementation, ordered by Name

**API — New files:**
- `Controllers/CompanyController.cs` — endpoints: `GET paged`, `GET {id}`, `POST`, `PUT {id}`

**API — Program.cs modified:**
- Added `AddScoped<ICompanyRepository, CompanyRepository>()`
- Added `AddScoped<ICompanyService, CompanyService>()`
- Added `AddScoped<IClientService, ClientService>()` (was missing)
- Cleaned up duplicate registrations and added section comments

**Endpoints added:**
- `GET  /api/company/paged?page=1&pageSize=20`
- `GET  /api/company/{id}`
- `POST /api/company`
- `PUT  /api/company/{id}`

---

## Note — Remaining issue in Program.cs

- ~~`AddHostedService<FollowUpNotificationWorker>()` is registered **twice**~~ — **Fixed** (2026-05-07)

---

## Miscellaneous *(2026-05-07)*

- Switched from Scalar to **Swagger UI** (Swashbuckle.AspNetCore). Removed `Scalar.AspNetCore` and `Microsoft.AspNetCore.OpenApi` packages. Swagger available at `/swagger`.
- Fixed duplicate `FollowUpNotificationWorker` registration in `Program.cs`.

---

## Phase 2 — Company CRUD & Onboarding *(completed 2026-05-07)*

**Domain — New files:**
- `Interfaces/ICompanyRepository.cs` — `GetAllAsync`, `GetByIdAsync`, `AddAsync`, `UpdateAsync`, `SaveChangesAsync`, `GetPagedAsync`

**Application — New files:**
- `Dtos/CompanyDto.cs` — response DTO (Id, Name, ContactEmail, Phone, Address)
- `Dtos/CreateCompanyRequest.cs` — create request DTO
- `Dtos/UpdateCompanyRequest.cs` — update request DTO
- `Services/Interfaces/ICompanyService.cs` — service interface
- `Services/CompanyService.cs` — implementation with `ToDto` mapping

**Infrastructure — New files:**
- `Repositories/CompanyRepository.cs` — EF Core implementation, ordered by Name

**API — New files:**
- `Controllers/CompanyController.cs` — `GET paged`, `GET {id}`, `POST`, `PUT {id}`

**API — Program.cs modified:**
- Added `AddScoped<ICompanyRepository, CompanyRepository>()`
- Added `AddScoped<ICompanyService, CompanyService>()`
- Added `AddScoped<IClientService, ClientService>()` (was missing)
- Cleaned up duplicate registrations and added section comments

---

## Phase 3 — Authentication & Authorization

### 3.1 User entity & auth setup *(completed 2026-05-07)*

**Domain — New files:**
- `Enums/Role.cs` — `SuperAdmin = 0`, `Admin = 1`, `User = 2`
- `Entities/User.cs` — `Id`, `Email`, `PasswordHash`, `CompanyId?`, `Role`, `IsActive`, `CreatedAt`
- `Interfaces/IUserRepository.cs` — `GetByIdAsync`, `GetByEmailAsync`, `GetByCompanyAsync`, `AddAsync`, `UpdateAsync`, `SaveChangesAsync`

**Infrastructure — New files:**
- `Repositories/UserRepository.cs` — EF Core implementation

**Infrastructure — AppDbContext modified:**
- Added `DbSet<User>`
- Added User entity configuration (unique email index, Role stored as int, FK to Company with `SetNull`)
- Added seed data: SuperAdmin user (`admin@act.local` / `Admin123!`, Role=SuperAdmin, CompanyId=null)

**API — Program.cs modified:**
- Added `AddScoped<IUserRepository, UserRepository>()`

**Migration:** `20260507195914_AddUserEntity`

---

### 3.2 JWT auth infrastructure *(completed 2026-05-08)*

**API — appsettings.json modified:**
- Added `JwtSettings` section: `Secret`, `Issuer`, `Audience`, `ExpiryMinutes` (480)

**Application — New files:**
- `Services/Interfaces/IJwtService.cs` — `GenerateToken(User user)`
- `Services/Interfaces/IPasswordHasher.cs` — `Hash(string)`, `Verify(string, string)`

**Infrastructure — New files:**
- `Services/JwtService.cs` — generates JWT with claims (sub, email, role, companyId)
- `Services/PasswordHasher.cs` — BCrypt wrapper

**NuGet packages added:**
- `Microsoft.AspNetCore.Authentication.JwtBearer` (API project)
- `BCrypt.Net-Next` (Infrastructure + Application projects)
- `System.IdentityModel.Tokens.Jwt` (Infrastructure project)
- `Microsoft.IdentityModel.Tokens` (Infrastructure project)

**API — Program.cs modified:**
- Added JWT authentication configuration (`AddAuthentication`, `AddJwtBearer`, `AddAuthorization`)
- Added `app.UseAuthentication()` before `app.UseAuthorization()`
- Registered `IJwtService`, `IPasswordHasher`

---

### 3.3 Auth endpoints *(completed 2026-05-08)*

**Application — New files:**
- `Dtos/LoginRequest.cs` — email + password
- `Dtos/RegisterRequest.cs` — email, password, companyId, role
- `Dtos/AuthResponseDto.cs` — token, email, role, companyId
- `Services/Interfaces/IAuthService.cs` — `LoginAsync`, `RegisterAsync`
- `Services/AuthService.cs` — validates credentials, hashes passwords, calls JwtService

**API — New files:**
- `Controllers/AuthController.cs`:
  - `POST /api/auth/login` — `[AllowAnonymous]`, returns JWT
  - `POST /api/auth/register` — `[Authorize(Roles = "SuperAdmin")]`, creates user for a company

**API — Program.cs modified:**
- Added `AddScoped<IAuthService, AuthService>()`

**Seeded SuperAdmin credentials (for testing):**
- Email: `admin@act.local`
- Password: `Admin123!`

---

### 3.4 Wire CompanyId from JWT into controllers *(completed 2026-05-08)*

**All controllers updated — replaced `HttpContext.Items["CompanyId"]` with JWT claims:**
- `ClientController.cs` — `[Authorize]`, `CompanyId` from `User.FindFirstValue("companyId")`
- `TreatmentController.cs` — same
- `TreatmentTypeController.cs` — same
- `FollowUpsController.cs` — same
- `FollowUpPeriodsController.cs` — `[Authorize]` added
- `Admin/BrandSettingsController.cs` — `[Authorize]` added

**Role-based access:**
- `CompanyController.cs` — `[Authorize(Roles = "SuperAdmin")]`
- `AuthController.cs` — `POST /auth/login` is `[AllowAnonymous]`, `POST /auth/register` is `[Authorize(Roles = "SuperAdmin")]`

**Swagger JWT support:**
- Added Bearer token security definition + requirement to SwaggerGen config
- Downgraded `Swashbuckle.AspNetCore` from v10.1.7 → v6.9.0 (v10 uses OpenApi v2 with incompatible namespace)

**API — Program.cs modified:**
- Added `using Microsoft.OpenApi.Models`
- Added `AddSecurityDefinition` + `AddSecurityRequirement` for JWT Bearer in Swagger

---

### 3.4a JWT claim mapping fix *(completed 2026-05-08)*

**Problem:** `[Authorize(Roles = "SuperAdmin")]` returned 401/403 because ASP.NET Core remapped JWT claim names.

**Fix — Program.cs:**
- Added `JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear()`
- Added `options.MapInboundClaims = false` on JwtBearer options
- Added `RoleClaimType = "role"` and `NameClaimType = "email"` to `TokenValidationParameters`

---

### 3.4b SuperAdmin sees all companies' data *(completed 2026-05-08)*

**Problem:** SuperAdmin has `CompanyId = null` (no company), so `int.Parse(null)` threw "Value cannot be null".

**Design change:** `companyId` is now `int?` throughout the stack:
- `null` = no filter, return all companies' data (SuperAdmin)
- `X` = filter by that company (Admin/User)

**Domain — Interfaces modified:**
- `IClientRepository.cs` — `GetAllAsync(int? companyId, ...)`, `GetPagedAsync(int? companyId, ...)`
- `ITreatmentRepository.cs` — `GetDueAsync(int?)`, `GetTodayAsync(int?)`, `GetPagedAsync(int?, ...)`
- `ITreatmentTypeRepository.cs` — `GetAllActiveAsync(int?)`, `GetPagedAsync(int?, ...)`

**Infrastructure — Repositories modified:**
- `ClientRepository.cs` — conditionally filters by companyId only when non-null
- `TreatmentRepository.cs` — same pattern
- `TreatmentTypeRepository.cs` — same pattern

**Application — Service interfaces + implementations modified:**
- `IClientService.cs` / `ClientService.cs` — `int?` for list/paged methods
- `ITreatmentService.cs` / `TreatmentService.cs` — `int?` for due/today/paged methods
- `ITreatmentTypeService.cs` / `TreatmentTypeService.cs` — `int?` for active/paged methods

**API — Controllers modified:**
- All controllers: `CompanyId` property returns `int?` — `null` when claim is missing (SuperAdmin), parsed int when present (Admin/User)
- Create endpoints: return `400 Bad Request` if `CompanyId` is null (SuperAdmin must specify a target company)

---

## Phase 4 — Branding & Settings *(completed 2026-05-09)*

**Application — New files:**
- `Dtos/BrandSettingsDto.cs` — response DTO (Id, CompanyId, PrimaryColor, SecondaryColor, AccentColor, Theme, LogoUrl)
- `Dtos/CreateBrandSettingsRequest.cs` — create request DTO
- `Dtos/UpdateBrandSettingsRequest.cs` — update request DTO

**Application — Modified files:**
- `Services/Interfaces/IBrandSettingsService.cs` — refactored to return `BrandSettingsDto` instead of entity; `CreateAsync` and `UpdateAsync` now accept DTOs + `companyId`
- `Services/BrandSettingsService.cs` — refactored: accepts DTOs, maps to/from entity with `ToDto()`, `companyId` set from parameter (not from entity input)

**API — Modified files:**
- `Controllers/Admin/BrandSettingsController.cs` — full refactor:
  - Uses DTOs instead of domain entities
  - `CompanyId` resolved from JWT claims (`int?` — null for SuperAdmin)
  - SuperAdmin can pass `?companyId=X` query param to target any company
  - Non-SuperAdmin users get `403 Forbid` if they try to access another company's branding
  - Endpoints: `GET /api/brandsettings`, `POST /api/brandsettings`, `PUT /api/brandsettings`

**Key design:**
- Admin/User → `CompanyId` auto-resolved from JWT, can only access own company
- SuperAdmin → must pass `?companyId=X`, can access any company

---

## Phase 5 — User Management *(completed 2026-05-10)*

**Application — New files:**
- `Dtos/UserDto.cs` — response DTO (Id, Email, CompanyId, Role, IsActive, CreatedAt)
- `Dtos/UpdateUserRequest.cs` — partial update (Role?, IsActive?)
- `Services/Interfaces/IUserService.cs` — `GetByCompanyAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeactivateAsync`
- `Services/UserService.cs` — implementation with password hashing and `ToDto` mapping

**API — New files:**
- `Controllers/Admin/UserController.cs`:
  - `GET    /api/user?companyId=X` — list users for a company (Admin: own company, SuperAdmin: any)
  - `GET    /api/user/{id}` — get single user
  - `POST   /api/user` — create user (uses existing `RegisterRequest` DTO)
  - `PUT    /api/user/{id}` — update role / active status
  - `DELETE /api/user/{id}` — soft deactivate (sets `IsActive = false`)

**API — Program.cs modified:**
- Added `AddScoped<IUserService, UserService>()`

**Access rules:**
| Action | SuperAdmin | Admin | User |
|--------|-----------|-------|------|
| List users | Any company (`?companyId=X`) | Own company only | ❌ |
| Create user | Any company, any role | Own company, cannot create SuperAdmin | ❌ |
| Update user | Any user | Own company, cannot promote to SuperAdmin | ❌ |
| Deactivate user | Any user | Own company only | ❌ |

---

## Phase 6 — Audit Trail & Login History *(completed 2026-05-10)*

**Domain — New files:**
- `Entities/AuditLog.cs` — UserId, UserEmail, CompanyId, Action, EntityType, EntityId, Details, Timestamp
- `Entities/LoginHistory.cs` — UserId, Email, CompanyId, IpAddress, UserAgent, Success, FailureReason, Timestamp
- `Interfaces/IAuditLogRepository.cs` — AddAsync, GetByCompanyAsync, GetAllAsync, GetPagedAsync
- `Interfaces/ILoginHistoryRepository.cs` — AddAsync, GetByUserAsync, GetByCompanyAsync, GetAllAsync, GetPagedAsync

**Infrastructure — New files:**
- `Repositories/AuditLogRepository.cs` — EF Core implementation with company filtering and pagination
- `Repositories/LoginHistoryRepository.cs` — EF Core implementation with company/user filtering and pagination

**Infrastructure — Modified files:**
- `Persistence/AppDbContext.cs`:
  - Added `DbSet<AuditLog>` and `DbSet<LoginHistory>`
  - Entity config: indexes on CompanyId, Timestamp, UserId for efficient querying

**Application — New files:**
- `Dtos/AuditLogDto.cs` — response DTO
- `Dtos/LoginHistoryDto.cs` — response DTO
- `Services/Interfaces/IAuditService.cs` — LogAsync, GetPagedAsync, GetLoginHistoryPagedAsync
- `Services/AuditService.cs` — implementation with ToDto mappings

**Application — Modified files:**
- `Services/Interfaces/IAuthService.cs` — LoginAsync now accepts `ipAddress` and `userAgent` params
- `Services/AuthService.cs` — records every login attempt to LoginHistory (success/failure with reason, IP, UserAgent)

**API — New files:**
- `Controllers/Admin/AuditController.cs`:
  - `GET /api/audit/logs?page=1&pageSize=50` — paginated audit trail
  - `GET /api/audit/logins?page=1&pageSize=50` — paginated login history
  - SuperAdmin sees all; Admin sees own company only

**API — Modified files:**
- `Controllers/AuthController.cs` — passes `RemoteIpAddress` and `User-Agent` header to LoginAsync
- `Program.cs` — registered IAuditLogRepository, ILoginHistoryRepository, IAuditService

**Migration:** `AddAuditLogAndLoginHistory`

**Login history records:**
- Success: userId, email, companyId, IP, UserAgent, `Success = true`
- Failure (user not found): email, `FailureReason = "User not found"`
- Failure (inactive): userId, email, `FailureReason = "Account inactive"`
- Failure (bad password): userId, email, `FailureReason = "Invalid password"`
