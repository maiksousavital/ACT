# ACT SaaS Implementation Plan

This checklist tracks the step-by-step implementation of SaaS features for the ACT platform.
Check off each item as it is completed. Execute phases in order — each phase keeps the app working.

> **Detailed plan**: See [plan-saasImplementation.prompt.md](./plan-saasImplementation.prompt.md)

---

## Phase 1 — Multi-Tenancy Foundation (no auth yet)

### 1.1 Add CompanyId to entities
- [x] Add `CompanyId` (nullable int) + FK to `Client` entity
- [x] Add `CompanyId` (nullable int) + FK to `TreatmentType` entity
- [x] Add `CompanyId` (nullable int) + FK to `Treatment` (optional — already scoped via Client, but explicit is safer)
- [x] Configure FKs and navigation properties in `AppDbContext`
- [x] Add EF migration

### 1.2 Backfill & make required
- [x] Seed a default Company (Id = 1) in migration if none exists
- [x] Backfill existing rows: `UPDATE Clients SET CompanyId = 1`, same for TreatmentType, Treatment
- [x] Alter `CompanyId` columns to non-nullable
- [x] Add EF migration, update database

### 1.3 Update repositories & services
- [x] Update `IClientRepository` / `ClientRepository` to accept and filter by `CompanyId`
- [x] Update `ITreatmentRepository` / `TreatmentRepository` to filter by `CompanyId`
- [x] Update `ITreatmentTypeRepository` / `TreatmentTypeRepository` to filter by `CompanyId`
- [x] Update corresponding services to pass `CompanyId` through

### 1.4 ~~Temporary CompanyId resolution~~ — SKIPPED
> **Skipped**: Controllers already default `CompanyId = 1` for dev/testing. Phase 3 (JWT auth) will replace this with `User.Claims` extraction directly — no throwaway middleware needed.

---

## Phase 2 — Company CRUD & Onboarding

- [x] Add `ICompanyRepository` + `CompanyRepository` (follow existing pattern)
- [x] Add `CompanyDto`, `CreateCompanyRequest`, `UpdateCompanyRequest` DTOs
- [x] Add `ICompanyService` + `CompanyService`
- [x] Add `CompanyController` with CRUD endpoints
- [x] Register repository and service in `Program.cs`
- [ ] Test company CRUD via Scalar

---

## Phase 3 — Authentication & Authorization (Option A: Owner-Provisioned)

> **Approach**: SuperAdmin (you) creates companies and their first Admin user. No public signup.
> **Option B (self-service signup)** will be added later in Phase 7 alongside billing — it's additive, not a refactor.

### 3.1 User entity & auth setup
- [x] Add `Role` enum (`SuperAdmin`, `Admin`, `User`) to `ACT.Domain/Enums/`
- [x] Add `User` entity (`Id`, `Email`, `PasswordHash`, `CompanyId?`, `Role`, `IsActive`, `CreatedAt`)
- [x] `CompanyId` is **nullable** — `null` for SuperAdmin, required for Admin/User
- [x] Add `IUserRepository` + `UserRepository`
- [x] Add `DbSet<User>` to `AppDbContext`, configure FK to Company
- [x] Add EF migration, update database
- [x] Seed a default SuperAdmin user (email: `admin@act.local`, hashed password)

### 3.2 JWT auth infrastructure
- [ ] Add `JwtSettings` config section to `appsettings.json` (Secret, Issuer, Audience, ExpiryMinutes)
- [ ] Add `IJwtService` + `JwtService` to generate tokens with claims (`UserId`, `Email`, `CompanyId`, `Role`)
- [ ] Install `Microsoft.AspNetCore.Authentication.JwtBearer` NuGet package
- [ ] Configure JWT authentication in `Program.cs`
- [ ] Add password hashing service (BCrypt via `BCrypt.Net-Next` package)

### 3.3 Auth endpoints
- [ ] Add `AuthController` with:
  - `POST /auth/login` — public, validates credentials, returns JWT
  - `POST /auth/register` — **SuperAdmin only**, creates a User for a given CompanyId
- [ ] Add `LoginRequest` and `RegisterRequest` DTOs
- [ ] Add `AuthResponseDto` (token, email, role, companyId)
- [ ] Test login with seeded SuperAdmin via Swagger

### 3.4 Wire CompanyId from JWT into controllers
- [ ] Replace hardcoded `CompanyId = 1` fallback in controllers with `User.Claims` extraction
- [ ] Add `[Authorize]` attribute to all controllers
- [ ] Add role-based policies (`SuperAdmin`, `Admin`, `User`)
- [ ] SuperAdmin endpoints (company CRUD, user creation) require `[Authorize(Roles = "SuperAdmin")]`
- [ ] Test login → CRUD flow end-to-end

### 3.5 Google / OAuth login (optional, after JWT works)
- [ ] Add ASP.NET Core External Login with Google provider
- [ ] Map external login to internal User + CompanyId
- [ ] Test OAuth flow

### Future: Option B — Self-Service Signup (Phase 7)
> When billing is ready, add a **public** `POST /auth/signup` endpoint that:
> 1. Creates a new Company
> 2. Creates an Admin user for that Company
> 3. Assigns a default Plan (Free)
> 4. Returns a JWT
>
> This is **additive** — no changes to existing auth code.

---

## Phase 4 — Branding & Settings

- [ ] Verify `BrandSettings` is already scoped to `CompanyId` ✓
- [ ] Update `BrandSettingsService` to use auth-based `CompanyId` (remove hardcoded values)
- [ ] Test brand CRUD per company via Scalar
- [ ] Test frontend applies company-specific branding

---

## Phase 5 — User Management

- [ ] Add endpoints: invite user, list users, update user role, remove user (scoped to company admin)
- [ ] Add `IUserService` + `UserService`
- [ ] Add `UserController` under `Controllers/Admin/`
- [ ] Test user invitation and management

---

## Phase 6 — API & Data Security

- [ ] Add company-scoping middleware (enforce `CompanyId` on every query automatically)
- [ ] Add rate limiting per company/user (`AspNetCoreRateLimit` or .NET 9 built-in)
- [ ] Add structured logging (Serilog recommended)
- [ ] Test data isolation: Company A cannot see Company B's data

---

## Phase 7 — Subscription & Billing (last)

- [ ] Add `Plan` entity (Free, Pro, Enterprise) with feature flags
- [ ] Add `Subscription` entity linked to Company
- [ ] Integrate Stripe (checkout session + webhooks)
- [ ] Gate features by plan (middleware or policy-based)
- [ ] Test subscription creation, upgrade, cancellation

---

## Phase 8 — Admin Portal

- [ ] Build super-admin endpoints (manage companies, users, plans, billing)
- [ ] Add `Controllers/Admin/SuperAdminController.cs`
- [ ] Test admin management

---

## Phase 9 — Frontend Integration

- [ ] Fetch user and company info on login
- [ ] Apply company branding (colours, logo) from `BrandSettings`
- [ ] Restrict UI features by role and plan
- [ ] Test company-aware UI

---

## Phase 10 — Documentation & Operations

- [ ] Document API endpoints (Scalar / OpenAPI)
- [ ] Document onboarding flow
- [ ] Deploy to cloud (Azure / AWS / GCP)
- [ ] Set up automated backups, monitoring, and alerting

---

## Key Decisions & Notes

| Topic | Decision |
|-------|----------|
| **IDs** | Integer (not GUID) |
| **Dapper** | Not needed now — EF Core is sufficient. Consider for complex reporting later. |
| **Treatment CompanyId** | Recommended explicit (not just via Client FK) for simpler queries |
| **Temp CompanyId** | Use `X-Company-Id` header until Phase 3 replaces it with JWT claims |
| **Google OAuth** | Add after basic JWT works (Phase 3.5) |
| **Auth approach** | Option A (owner-provisioned) — SuperAdmin creates companies + first users |
| **Self-service signup** | Option B — deferred to Phase 7, additive (new endpoint, no refactor) |
| **Roles** | `SuperAdmin` (no company), `Admin` (company owner), `User` (company staff) |

---

## Pitfalls to Avoid

1. **Adding CompanyId as non-nullable immediately** — existing rows will fail the migration. Always add as nullable first, backfill, then make required.
2. **Multi-tenancy before auth** — controllers default `CompanyId = 1` for dev. Phase 3 replaces with JWT claims.
3. **Billing too early** — complex and unnecessary until multi-tenancy + auth are solid. Keep it last.
4. **Big-bang refactors** — do one phase at a time, test, commit, then move on.

---

**Instructions:**
- Execute phases in order (1 → 10). Each phase keeps the app deployable.
- Mark items as complete by changing `[ ]` to `[x]`.
- Add notes, links, or dates as needed.
