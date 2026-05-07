# ACT SaaS Implementation Plan

This checklist tracks the step-by-step implementation of SaaS features for the ACT platform.
Check off each item as it is completed. Execute phases in order — each phase keeps the app working.

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

## Phase 3 — Authentication & Authorization

### 3.1 User entity & auth setup
- [ ] Add `User` entity (`Id`, `Email`, `PasswordHash`, `CompanyId`, `Role`)
- [ ] Add `IUserRepository` + `UserRepository`
- [ ] Integrate ASP.NET Core Identity or custom JWT-based auth
- [ ] Add EF migration, update database

### 3.2 Auth endpoints
- [ ] Add `POST /auth/register` (creates user + optionally a company)
- [ ] Add `POST /auth/login` (returns JWT with `CompanyId` and `Role` claims)
- [ ] Test registration and login via Scalar

### 3.3 Wire CompanyId from JWT into controllers
- [ ] Replace hardcoded `CompanyId = 1` fallback in controllers with `User.Claims` extraction
- [ ] `CompanyId` now resolved from authenticated user's token
- [ ] Add `[Authorize]` attribute to all controllers
- [ ] Add role-based policies (Admin, User)
- [ ] Test login → CRUD flow end-to-end

### 3.4 Google / OAuth login (optional, after JWT works)
- [ ] Add ASP.NET Core External Login with Google provider
- [ ] Map external login to internal User + CompanyId
- [ ] Test OAuth flow

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
| **Temp CompanyId** | Skipped middleware — controllers default to `1` until Phase 3 adds JWT claims |
| **Google OAuth** | Add after basic JWT works (Phase 3.4) |

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

