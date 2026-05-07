# Copilot Instructions for ACT Project

## What is ACT?

ACT (Automated Client Treatments) is a SaaS platform for clinics to manage clients, treatments, treatment types, and automated follow-up reminders. It supports multi-tenancy (Company entity), brand/theme customisation per company, and paginated lists.

## Tech Stack

- **Backend**: ASP.NET Core 9, Entity Framework Core, SQLite
- **Architecture**: Clean Architecture (Domain → Application → Infrastructure → API)
- **Frontend**: React (separate repo/project)
- **ORM**: EF Core (Code First with migrations)
- **IDs**: Integer (not GUID)

## Solution Structure

```
src/
  ACT.Domain/          # Entities, Enums, Interfaces (zero dependencies)
  ACT.Application/     # DTOs, Service interfaces & implementations (depends on Domain)
  ACT.Infrastructure/  # Repositories, DbContext, Migrations (depends on Application + Domain)
  ACT.API/             # Controllers, Program.cs DI setup (depends on all)
tests/
  ACT.Tests/           # Unit and integration tests
```

## Key Conventions

1. **Abstraction**: Controllers → Services (interface) → Repositories (interface). Never call repositories or DbContext from controllers directly.
2. **Interfaces**: Always in a dedicated `Interfaces/` subfolder.
3. **DTOs**: All API input/output uses DTOs in `ACT.Application/Dtos/`. Never expose domain entities directly.
4. **Async/Await**: All I/O methods must be async with proper await.
5. **Pagination**: Use `PagedResult<T>` DTO. All list endpoints support `page` and `pageSize` query params.
6. **DI Registration**: All services and repositories registered as `AddScoped<>` in `Program.cs`.
7. **Migrations**: Run from `ACT.Infrastructure` project using `--startup-project` pointing to `ACT.API`.

## Adding a New Feature (Checklist)

1. `ACT.Domain/Entities/MyEntity.cs` — entity with int Id
2. `ACT.Domain/Interfaces/IMyEntityRepository.cs` — repository interface
3. `ACT.Application/Dtos/MyEntityDto.cs` — response DTO
4. `ACT.Application/Dtos/CreateMyEntityRequest.cs` — create request DTO
5. `ACT.Application/Dtos/UpdateMyEntityRequest.cs` — update request DTO
6. `ACT.Application/Services/Interfaces/IMyEntityService.cs` — service interface
7. `ACT.Application/Services/MyEntityService.cs` — service implementation
8. `ACT.Infrastructure/Repositories/MyEntityRepository.cs` — repository implementation
9. `ACT.Infrastructure/Persistence/AppDbContext.cs` — add `DbSet<MyEntity>`
10. `ACT.API/Controllers/MyEntityController.cs` — CRUD endpoints
11. `Program.cs` — register `IMyEntityRepository` and `IMyEntityService`
12. Add EF migration: `dotnet ef migrations add AddMyEntity --project src/ACT.Infrastructure --startup-project src/ACT.API`
13. Update DB: `dotnet ef database update --project src/ACT.Infrastructure --startup-project src/ACT.API`

## Existing Entities

- **Company** — multi-tenant root entity (clinic/business)
- **Client** — belongs to a Company
- **TreatmentType** — defines treatment + follow-up interval (via `FollowUpPeriod` enum in days)
- **Treatment** — links Client + TreatmentType, tracks follow-up dates
- **BrandSettings** — per-Company theme/colors/logo configuration

## Enum: FollowUpPeriod

Stored as days: 1 Week (7), 2 Weeks (14), 3 Weeks (21), 4 Weeks (28), 3 Months (90), 6 Months (180), 1 Year (365). Returned to frontend for dropdowns via DTO, no dedicated controller.

## Common Patterns

- **GetById** returns a DTO (never the entity).
- **Circular references** are avoided by using DTOs without navigation properties.
- **Global query filters** (e.g., soft-delete) — configure navigations as optional if filtered.
- **JSON serialization** — use `ReferenceHandler.IgnoreCycles` only as last resort; prefer DTOs.

## Running the App

```bash
cd src/ACT.API
dotnet run
```

API available at `http://localhost:5105` (HTTP). Swagger UI at `/swagger`.

