using System.Security.Claims;
using ACT.API.Controllers;
using ACT.Application.Dtos;
using ACT.Application.Services;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Infrastructure.Persistence;
using ACT.Infrastructure.Repositories;
using ACT.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ACT.Tests;

/// <summary>
/// Regression tests for the A1 IDOR fix and the B1 EF Core tenant query filter: an authenticated
/// user from Company A must never be able to read, modify, or delete Company B's clients,
/// treatments, or treatment types — whether through the controller's explicit CompanyId check
/// (A1) or, with that check hypothetically removed, through the DbContext-level global query
/// filter alone (B1). Each test builds its own AppDbContext bound to a specific caller's
/// ITenantContext, mirroring how a real request gets a freshly-scoped DbContext per request.
/// </summary>
public class TenantIsolationTests
{
    private const int CompanyA = 1;
    private const int CompanyB = 2;

    private class FakeTenantContext : ITenantContext
    {
        public int? CompanyId { get; init; }
        public bool IsSuperAdmin { get; init; }

        public static readonly FakeTenantContext System = new() { IsSuperAdmin = true };
        public static FakeTenantContext For(int companyId) => new() { CompanyId = companyId };
    }

    private static AppDbContext BuildContext(string dbName, ITenantContext tenantContext)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options, tenantContext);
    }

    private class World
    {
        public required string DbName;
        public required Client ClientA;
        public required Client ClientB;
        public required TreatmentType TypeA;
        public required TreatmentType TypeB;
        public required Treatment TreatmentA;
        public required Treatment TreatmentB;
    }

    /// <summary>
    /// Seeds fixture data for two companies using an unrestricted (SuperAdmin) context — a real
    /// request would never seed data this way, but the fixture needs to write both companies'
    /// rows before any tenant-scoped context can read either of them.
    /// </summary>
    private static World BuildWorld()
    {
        var dbName = Guid.NewGuid().ToString();
        using var seedContext = BuildContext(dbName, FakeTenantContext.System);

        seedContext.Companies.AddRange(
            new Company { Id = CompanyA, Name = "Company A" },
            new Company { Id = CompanyB, Name = "Company B" });

        var clientA = new Client { Id = 1, FirstName = "Alice", LastName = "Anderson", CompanyId = CompanyA };
        var clientB = new Client { Id = 2, FirstName = "Bob", LastName = "Brown", CompanyId = CompanyB };
        seedContext.Clients.AddRange(clientA, clientB);

        var typeA = new TreatmentType { Id = 1, Name = "Botox", FollowUpIntervalDays = 90, IsActive = true, CompanyId = CompanyA };
        var typeB = new TreatmentType { Id = 2, Name = "Filler", FollowUpIntervalDays = 180, IsActive = true, CompanyId = CompanyB };
        seedContext.TreatmentTypes.AddRange(typeA, typeB);

        var treatmentA = new Treatment
        {
            Id = 1,
            ClientId = clientA.Id,
            TreatmentTypeId = typeA.Id,
            TreatmentDate = DateTime.UtcNow,
            NextFollowUpDate = DateTime.UtcNow.AddDays(90),
            CompanyId = CompanyA
        };
        var treatmentB = new Treatment
        {
            Id = 2,
            ClientId = clientB.Id,
            TreatmentTypeId = typeB.Id,
            TreatmentDate = DateTime.UtcNow,
            NextFollowUpDate = DateTime.UtcNow.AddDays(180),
            CompanyId = CompanyB
        };
        seedContext.Treatments.AddRange(treatmentA, treatmentB);

        seedContext.SaveChanges();

        return new World
        {
            DbName = dbName,
            ClientA = clientA,
            ClientB = clientB,
            TypeA = typeA,
            TypeB = typeB,
            TreatmentA = treatmentA,
            TreatmentB = treatmentB
        };
    }

    private sealed class Scope : IDisposable
    {
        public required AppDbContext Context;
        public required IClientService ClientService;
        public required ITreatmentService TreatmentService;
        public required ITreatmentTypeService TreatmentTypeService;
        public required ClientRepository ClientRepository;
        public required TreatmentRepository TreatmentRepository;
        public required TreatmentTypeRepository TreatmentTypeRepository;
        public void Dispose() => Context.Dispose();
    }

    /// <summary>
    /// Builds a fresh AppDbContext (and services on top of it) scoped to one caller's tenant —
    /// same in-memory database, different query-filter binding. This is what DI hands each
    /// controller action a real request: a new scoped DbContext resolved for that caller only.
    /// </summary>
    private static Scope ScopeFor(string dbName, int? companyId, bool isSuperAdmin = false)
    {
        var tenantContext = isSuperAdmin ? FakeTenantContext.System : FakeTenantContext.For(companyId!.Value);
        var context = BuildContext(dbName, tenantContext);
        var clientRepo = new ClientRepository(context);
        var treatmentRepo = new TreatmentRepository(context);
        var typeRepo = new TreatmentTypeRepository(context);

        var auditService = new NoopAuditService();
        return new Scope
        {
            Context = context,
            ClientService = new ClientService(clientRepo, auditService),
            TreatmentService = new TreatmentService(treatmentRepo, clientRepo, typeRepo, auditService),
            TreatmentTypeService = new TreatmentTypeService(typeRepo, auditService),
            ClientRepository = clientRepo,
            TreatmentRepository = treatmentRepo,
            TreatmentTypeRepository = typeRepo
        };
    }

    private static void SetUser(ControllerBase controller, int? companyId, string role = "Admin")
    {
        var claims = new List<Claim> { new("role", role) };
        if (companyId.HasValue)
            claims.Add(new Claim("companyId", companyId.Value.ToString()));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")) }
        };
    }

    // ── ClientController ─────────────────────────────────────────────────────
    // Cross-company single-record access now 404s rather than 403: the query filter hides the
    // row before the controller's explicit Forbid check ever sees it, which also means a cross-
    // tenant caller can no longer distinguish "exists but isn't yours" from "doesn't exist".

    [Fact]
    public async Task Client_GetById_CrossCompany_ReturnsNotFound()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);
        var controller = new ClientController(scope.ClientService);
        SetUser(controller, CompanyA);

        var result = await controller.GetById(world.ClientB.Id);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Client_GetById_SameCompany_ReturnsOk()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);
        var controller = new ClientController(scope.ClientService);
        SetUser(controller, CompanyA);

        var result = await controller.GetById(world.ClientA.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ClientDto>(ok.Value);
        Assert.Equal(world.ClientA.Id, dto.Id);
    }

    [Fact]
    public async Task Client_Update_CrossCompany_ReturnsNotFoundAndDoesNotMutate()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);
        var controller = new ClientController(scope.ClientService);
        SetUser(controller, CompanyA);

        var request = new UpdateClientRequest { FirstName = "Hacked", LastName = "Name" };
        var result = await controller.Update(world.ClientB.Id, request);

        Assert.IsType<NotFoundResult>(result.Result);
        using var verify = BuildContext(world.DbName, FakeTenantContext.System);
        var untouched = await verify.Clients.AsNoTracking().FirstAsync(c => c.Id == world.ClientB.Id);
        Assert.Equal("Bob", untouched.FirstName);
    }

    [Fact]
    public async Task Client_Update_SameCompany_ReturnsOk()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);
        var controller = new ClientController(scope.ClientService);
        SetUser(controller, CompanyA);

        var request = new UpdateClientRequest { FirstName = "Alicia", LastName = "Anderson" };
        var result = await controller.Update(world.ClientA.Id, request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ClientDto>(ok.Value);
        Assert.Equal("Alicia", dto.FirstName);
    }

    [Fact]
    public async Task Client_Delete_CrossCompany_ReturnsNotFoundAndDoesNotDelete()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);
        var controller = new ClientController(scope.ClientService);
        SetUser(controller, CompanyA);

        var result = await controller.Delete(world.ClientB.Id);

        Assert.IsType<NotFoundResult>(result);
        using var verify = BuildContext(world.DbName, FakeTenantContext.System);
        var untouched = await verify.Clients.AsNoTracking().FirstAsync(c => c.Id == world.ClientB.Id);
        Assert.False(untouched.IsDeleted);
    }

    [Fact]
    public async Task Client_GetById_SuperAdmin_BypassesCompanyCheck()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, companyId: null, isSuperAdmin: true);
        var controller = new ClientController(scope.ClientService);
        SetUser(controller, companyId: null, role: "SuperAdmin");

        var result = await controller.GetById(world.ClientB.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<ClientDto>(ok.Value);
    }

    // ── TreatmentsController ─────────────────────────────────────────────────

    [Fact]
    public async Task Treatment_GetById_CrossCompany_ReturnsNotFound()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);
        var controller = new TreatmentsController(scope.TreatmentService);
        SetUser(controller, CompanyA);

        var result = await controller.GetById(world.TreatmentB.Id);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Treatment_Update_CrossCompany_ReturnsNotFoundAndDoesNotMutate()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);
        var controller = new TreatmentsController(scope.TreatmentService);
        SetUser(controller, CompanyA);

        var request = new UpdateTreatmentRequest
        {
            ClientId = world.ClientB.Id,
            TreatmentTypeId = world.TypeB.Id,
            TreatmentDate = DateTime.UtcNow,
            Notes = "hacked"
        };
        var result = await controller.Update(world.TreatmentB.Id, request);

        Assert.IsType<NotFoundResult>(result);
        using var verify = BuildContext(world.DbName, FakeTenantContext.System);
        var untouched = await verify.Treatments.AsNoTracking().FirstAsync(t => t.Id == world.TreatmentB.Id);
        Assert.Null(untouched.Notes);
    }

    [Fact]
    public async Task Treatment_GetByClient_CrossCompanyClient_ReturnsEmpty()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);
        var controller = new TreatmentsController(scope.TreatmentService);
        SetUser(controller, CompanyA);

        var result = await controller.GetByClient(world.ClientB.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        var treatments = Assert.IsAssignableFrom<IEnumerable<TreatmentDto>>(ok.Value);
        Assert.Empty(treatments);
    }

    [Fact]
    public async Task Treatment_GetByClient_SameCompanyClient_ReturnsResults()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);
        var controller = new TreatmentsController(scope.TreatmentService);
        SetUser(controller, CompanyA);

        var result = await controller.GetByClient(world.ClientA.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        var treatments = Assert.IsAssignableFrom<IEnumerable<TreatmentDto>>(ok.Value);
        Assert.Single(treatments);
    }

    // ── TreatmentTypeController ──────────────────────────────────────────────

    [Fact]
    public async Task TreatmentType_GetById_CrossCompany_ReturnsNotFound()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);
        var controller = new TreatmentTypeController(scope.TreatmentTypeService);
        SetUser(controller, CompanyA);

        var result = await controller.GetById(world.TypeB.Id);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task TreatmentType_Update_CrossCompany_ReturnsNotFoundAndDoesNotMutate()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);
        var controller = new TreatmentTypeController(scope.TreatmentTypeService);
        SetUser(controller, CompanyA);

        var request = new UpdateTreatmentTypeRequest { Name = "Hacked", FollowUpIntervalDays = 1, IsActive = true };
        var result = await controller.Update(world.TypeB.Id, request);

        Assert.IsType<NotFoundResult>(result.Result);
        using var verify = BuildContext(world.DbName, FakeTenantContext.System);
        var untouched = await verify.TreatmentTypes.AsNoTracking().FirstAsync(t => t.Id == world.TypeB.Id);
        Assert.Equal("Filler", untouched.Name);
    }

    [Fact]
    public async Task TreatmentType_Update_SameCompany_ReturnsOk()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);
        var controller = new TreatmentTypeController(scope.TreatmentTypeService);
        SetUser(controller, CompanyA);

        var request = new UpdateTreatmentTypeRequest { Name = "Botox Renamed", FollowUpIntervalDays = 90, IsActive = true };
        var result = await controller.Update(world.TypeA.Id, request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<TreatmentTypeDto>(ok.Value);
        Assert.Equal("Botox Renamed", dto.Name);
    }

    // ── TreatmentService — cross-tenant reference validation ────────────────

    [Fact]
    public async Task TreatmentService_CreateAsync_ClientFromOtherCompany_ThrowsKeyNotFound()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);
        var request = new CreateTreatmentRequest
        {
            ClientId = world.ClientB.Id,
            TreatmentTypeId = world.TypeA.Id,
            TreatmentDate = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => scope.TreatmentService.CreateAsync(CompanyA, request, null, "test@test.com"));
    }

    [Fact]
    public async Task TreatmentService_CreateAsync_TreatmentTypeFromOtherCompany_ThrowsKeyNotFound()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);
        var request = new CreateTreatmentRequest
        {
            ClientId = world.ClientA.Id,
            TreatmentTypeId = world.TypeB.Id,
            TreatmentDate = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => scope.TreatmentService.CreateAsync(CompanyA, request, null, "test@test.com"));
    }

    [Fact]
    public async Task TreatmentService_UpdateAsync_ReassignToOtherCompanyClient_ThrowsKeyNotFound()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);
        var request = new UpdateTreatmentRequest
        {
            ClientId = world.ClientB.Id,
            TreatmentTypeId = world.TreatmentA.TreatmentTypeId,
            TreatmentDate = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => scope.TreatmentService.UpdateAsync(world.TreatmentA.Id, request, null, "test@test.com"));
    }

    [Fact]
    public async Task TreatmentService_UpdateAsync_ReassignToOtherCompanyTreatmentType_ThrowsKeyNotFound()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);
        var request = new UpdateTreatmentRequest
        {
            ClientId = world.TreatmentA.ClientId,
            TreatmentTypeId = world.TypeB.Id,
            TreatmentDate = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => scope.TreatmentService.UpdateAsync(world.TreatmentA.Id, request, null, "test@test.com"));
    }

    [Fact]
    public async Task TreatmentService_UpdateAsync_SameCompanyReferences_Succeeds()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);
        var request = new UpdateTreatmentRequest
        {
            ClientId = world.TreatmentA.ClientId,
            TreatmentTypeId = world.TreatmentA.TreatmentTypeId,
            TreatmentDate = DateTime.UtcNow,
            Notes = "updated"
        };

        var result = await scope.TreatmentService.UpdateAsync(world.TreatmentA.Id, request, null, "test@test.com");

        Assert.NotNull(result);
        Assert.Equal("updated", result!.Notes);
    }

    // ── B1 — EF Core global query filter, independent of any controller check ──────────────

    [Fact]
    public async Task QueryFilter_ClientRepository_CrossCompany_HidesRowEvenWithoutControllerCheck()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);

        // Calling the repository directly — no controller, no explicit CompanyId comparison —
        // proves the tenant boundary holds at the data layer itself, not just by convention.
        var result = await scope.ClientRepository.GetByIdAsync(world.ClientB.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task QueryFilter_TreatmentRepository_CrossCompany_HidesRow()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);

        var result = await scope.TreatmentRepository.GetByIdAsync(world.TreatmentB.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task QueryFilter_TreatmentTypeRepository_CrossCompany_HidesRow()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, CompanyA);

        var result = await scope.TreatmentTypeRepository.GetByIdAsync(world.TypeB.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task QueryFilter_SuperAdmin_SeesBothCompanies()
    {
        var world = BuildWorld();
        using var scope = ScopeFor(world.DbName, companyId: null, isSuperAdmin: true);

        var clients = await scope.Context.Clients.ToListAsync();

        Assert.Contains(clients, c => c.Id == world.ClientA.Id);
        Assert.Contains(clients, c => c.Id == world.ClientB.Id);
    }

    [Fact]
    public async Task QueryFilter_Client_StillHidesSoftDeletedRowsWithinSameCompany()
    {
        var world = BuildWorld();
        using (var deleteScope = ScopeFor(world.DbName, CompanyA))
        {
            var client = await deleteScope.Context.Clients.FirstAsync(c => c.Id == world.ClientA.Id);
            client.IsDeleted = true;
            await deleteScope.Context.SaveChangesAsync();
        }

        using var scope = ScopeFor(world.DbName, CompanyA);
        var result = await scope.ClientRepository.GetByIdAsync(world.ClientA.Id);

        Assert.Null(result);
    }
}
