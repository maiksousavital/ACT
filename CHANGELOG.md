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

- `AddHostedService<FollowUpNotificationWorker>()` is registered **twice** (lines 30 and 34). Should be registered once.

