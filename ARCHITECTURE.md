# ACT Project Architecture & Coding Guidelines

## Solution Structure

- **src/ACT.Domain/**: Entities and interfaces (no dependencies)
- **src/ACT.Application/**: DTOs, services, interfaces (depends only on Domain)
- **src/ACT.Infrastructure/**: Repositories, EF Core, persistence (depends on Application + Domain)
- **src/ACT.API/**: Controllers, DI, API endpoints (depends on all layers)
- **tests/ACT.Tests/**: Unit/integration tests

## Clean Architecture Principles

- Controllers depend on services (abstractions), not repositories or DbContext directly.
- Services depend on repositories (abstractions), not EF Core directly.
- Repositories implement interfaces and handle all persistence logic.
- DTOs are used for all API input/output (never expose entities directly).

## Adding a New Feature

1. **Domain Layer**: Add new entity/interface if needed.
2. **Application Layer**: Add DTOs, service interface, and implementation.
3. **Infrastructure Layer**: Add repository interface and implementation.
4. **API Layer**: Add controller, inject service, use DTOs for input/output.
5. **Register** new services and repositories in `Program.cs` for DI.
6. **Migrations**: If the model changes, add and apply a new EF migration.
7. **Testing**: Add/extend tests in `tests/ACT.Tests/`.

## Example: Adding a "Product" Feature

- `ACT.Domain/Entities/Product.cs`
- `ACT.Domain/Interfaces/IProductRepository.cs`
- `ACT.Application/Dtos/ProductDto.cs`
- `ACT.Application/Services/Interfaces/IProductService.cs`
- `ACT.Application/Services/ProductService.cs`
- `ACT.Infrastructure/Repositories/ProductRepository.cs`
- `ACT.API/Controllers/ProductController.cs`
- Register in `Program.cs`:
  ```csharp
  builder.Services.AddScoped<IProductRepository, ProductRepository>();
  builder.Services.AddScoped<IProductService, ProductService>();
  ```

## Naming & Folder Conventions

- Place interfaces in `Interfaces/` folders.
- Place DTOs in `Dtos/` folders.
- Place services in `Services/` folders.
- Place repositories in `Repositories/` folders.
- Place admin/configuration controllers in `Controllers/Admin/`.

## Other Best Practices

- Use async/await everywhere for I/O.
- Use dependency injection for all services and repositories.
- Use DTOs for all API input/output.
- Keep controllers thin; put business logic in services.

## How to Use This Guide

- Reference this file when adding new features or refactoring.
- Ensure all new code follows the structure, naming, and best practices described here.
- When using AI tools, paste this file or reference it to ensure consistency.

