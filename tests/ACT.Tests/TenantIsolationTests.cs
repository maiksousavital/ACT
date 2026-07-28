using System.Security.Claims;
using ACT.API.Controllers;
using ACT.Application.Dtos;
using ACT.Application.Services;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Infrastructure.Persistence;
using ACT.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ACT.Tests;

/// <summary>
/// Regression tests for the A1 IDOR fix: an authenticated user from Company A must never be able
/// to read, modify, or delete Company B's clients, treatments, or treatment types via direct ID
/// access — and Treatment create/update must reject cross-company Client/TreatmentType references.
/// </summary>
public class TenantIsolationTests
{
    private const int CompanyA = 1;
    private const int CompanyB = 2;

    private static AppDbContext BuildContext()
    {
        // Unique DB name per test — no EnsureCreated() call, so the model's HasData seed
        // (Company/TreatmentType/User with Id=1) never gets inserted and can't collide with
        // the fixture data below.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private class Fixture
    {
        public required AppDbContext Context;
        public required IClientService ClientService;
        public required ITreatmentService TreatmentService;
        public required ITreatmentTypeService TreatmentTypeService;
        public required Client ClientA;
        public required Client ClientB;
        public required TreatmentType TypeA;
        public required TreatmentType TypeB;
        public required Treatment TreatmentA;
        public required Treatment TreatmentB;
    }

    private static Fixture BuildFixture()
    {
        var context = BuildContext();

        context.Companies.AddRange(
            new Company { Id = CompanyA, Name = "Company A" },
            new Company { Id = CompanyB, Name = "Company B" });

        var clientA = new Client { Id = 1, FirstName = "Alice", LastName = "Anderson", CompanyId = CompanyA };
        var clientB = new Client { Id = 2, FirstName = "Bob", LastName = "Brown", CompanyId = CompanyB };
        context.Clients.AddRange(clientA, clientB);

        var typeA = new TreatmentType { Id = 1, Name = "Botox", FollowUpIntervalDays = 90, IsActive = true, CompanyId = CompanyA };
        var typeB = new TreatmentType { Id = 2, Name = "Filler", FollowUpIntervalDays = 180, IsActive = true, CompanyId = CompanyB };
        context.TreatmentTypes.AddRange(typeA, typeB);

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
        context.Treatments.AddRange(treatmentA, treatmentB);

        context.SaveChanges();

        var clientRepo = new ClientRepository(context);
        var treatmentRepo = new TreatmentRepository(context);
        var typeRepo = new TreatmentTypeRepository(context);

        return new Fixture
        {
            Context = context,
            ClientService = new ClientService(clientRepo),
            TreatmentService = new TreatmentService(treatmentRepo, clientRepo, typeRepo),
            TreatmentTypeService = new TreatmentTypeService(typeRepo),
            ClientA = clientA,
            ClientB = clientB,
            TypeA = typeA,
            TypeB = typeB,
            TreatmentA = treatmentA,
            TreatmentB = treatmentB
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

    [Fact]
    public async Task Client_GetById_CrossCompany_ReturnsForbid()
    {
        var f = BuildFixture();
        var controller = new ClientController(f.ClientService);
        SetUser(controller, CompanyA);

        var result = await controller.GetById(f.ClientB.Id);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task Client_GetById_SameCompany_ReturnsOk()
    {
        var f = BuildFixture();
        var controller = new ClientController(f.ClientService);
        SetUser(controller, CompanyA);

        var result = await controller.GetById(f.ClientA.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ClientDto>(ok.Value);
        Assert.Equal(f.ClientA.Id, dto.Id);
    }

    [Fact]
    public async Task Client_Update_CrossCompany_ReturnsForbidAndDoesNotMutate()
    {
        var f = BuildFixture();
        var controller = new ClientController(f.ClientService);
        SetUser(controller, CompanyA);

        var request = new UpdateClientRequest { FirstName = "Hacked", LastName = "Name" };
        var result = await controller.Update(f.ClientB.Id, request);

        Assert.IsType<ForbidResult>(result.Result);
        var untouched = await f.Context.Clients.AsNoTracking().FirstAsync(c => c.Id == f.ClientB.Id);
        Assert.Equal("Bob", untouched.FirstName);
    }

    [Fact]
    public async Task Client_Update_SameCompany_ReturnsOk()
    {
        var f = BuildFixture();
        var controller = new ClientController(f.ClientService);
        SetUser(controller, CompanyA);

        var request = new UpdateClientRequest { FirstName = "Alicia", LastName = "Anderson" };
        var result = await controller.Update(f.ClientA.Id, request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ClientDto>(ok.Value);
        Assert.Equal("Alicia", dto.FirstName);
    }

    [Fact]
    public async Task Client_Delete_CrossCompany_ReturnsForbidAndDoesNotDelete()
    {
        var f = BuildFixture();
        var controller = new ClientController(f.ClientService);
        SetUser(controller, CompanyA);

        var result = await controller.Delete(f.ClientB.Id);

        Assert.IsType<ForbidResult>(result);
        var untouched = await f.Context.Clients.AsNoTracking().FirstAsync(c => c.Id == f.ClientB.Id);
        Assert.False(untouched.IsDeleted);
    }

    [Fact]
    public async Task Client_GetById_SuperAdmin_BypassesCompanyCheck()
    {
        var f = BuildFixture();
        var controller = new ClientController(f.ClientService);
        SetUser(controller, companyId: null, role: "SuperAdmin");

        var result = await controller.GetById(f.ClientB.Id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<ClientDto>(ok.Value);
    }

    // ── TreatmentsController ─────────────────────────────────────────────────

    [Fact]
    public async Task Treatment_GetById_CrossCompany_ReturnsForbid()
    {
        var f = BuildFixture();
        var controller = new TreatmentsController(f.TreatmentService);
        SetUser(controller, CompanyA);

        var result = await controller.GetById(f.TreatmentB.Id);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task Treatment_Update_CrossCompany_ReturnsForbidAndDoesNotMutate()
    {
        var f = BuildFixture();
        var controller = new TreatmentsController(f.TreatmentService);
        SetUser(controller, CompanyA);

        var request = new UpdateTreatmentRequest
        {
            ClientId = f.ClientB.Id,
            TreatmentTypeId = f.TypeB.Id,
            TreatmentDate = DateTime.UtcNow,
            Notes = "hacked"
        };
        var result = await controller.Update(f.TreatmentB.Id, request);

        Assert.IsType<ForbidResult>(result);
        var untouched = await f.Context.Treatments.AsNoTracking().FirstAsync(t => t.Id == f.TreatmentB.Id);
        Assert.Null(untouched.Notes);
    }

    [Fact]
    public async Task Treatment_GetByClient_CrossCompanyClient_ReturnsEmpty()
    {
        var f = BuildFixture();
        var controller = new TreatmentsController(f.TreatmentService);
        SetUser(controller, CompanyA);

        var result = await controller.GetByClient(f.ClientB.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        var treatments = Assert.IsAssignableFrom<IEnumerable<TreatmentDto>>(ok.Value);
        Assert.Empty(treatments);
    }

    [Fact]
    public async Task Treatment_GetByClient_SameCompanyClient_ReturnsResults()
    {
        var f = BuildFixture();
        var controller = new TreatmentsController(f.TreatmentService);
        SetUser(controller, CompanyA);

        var result = await controller.GetByClient(f.ClientA.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        var treatments = Assert.IsAssignableFrom<IEnumerable<TreatmentDto>>(ok.Value);
        Assert.Single(treatments);
    }

    // ── TreatmentTypeController ──────────────────────────────────────────────

    [Fact]
    public async Task TreatmentType_GetById_CrossCompany_ReturnsForbid()
    {
        var f = BuildFixture();
        var controller = new TreatmentTypeController(f.TreatmentTypeService);
        SetUser(controller, CompanyA);

        var result = await controller.GetById(f.TypeB.Id);

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task TreatmentType_Update_CrossCompany_ReturnsForbidAndDoesNotMutate()
    {
        var f = BuildFixture();
        var controller = new TreatmentTypeController(f.TreatmentTypeService);
        SetUser(controller, CompanyA);

        var request = new UpdateTreatmentTypeRequest { Name = "Hacked", FollowUpIntervalDays = 1, IsActive = true };
        var result = await controller.Update(f.TypeB.Id, request);

        Assert.IsType<ForbidResult>(result.Result);
        var untouched = await f.Context.TreatmentTypes.AsNoTracking().FirstAsync(t => t.Id == f.TypeB.Id);
        Assert.Equal("Filler", untouched.Name);
    }

    [Fact]
    public async Task TreatmentType_Update_SameCompany_ReturnsOk()
    {
        var f = BuildFixture();
        var controller = new TreatmentTypeController(f.TreatmentTypeService);
        SetUser(controller, CompanyA);

        var request = new UpdateTreatmentTypeRequest { Name = "Botox Renamed", FollowUpIntervalDays = 90, IsActive = true };
        var result = await controller.Update(f.TypeA.Id, request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<TreatmentTypeDto>(ok.Value);
        Assert.Equal("Botox Renamed", dto.Name);
    }

    // ── TreatmentService — cross-tenant reference validation ────────────────

    [Fact]
    public async Task TreatmentService_CreateAsync_ClientFromOtherCompany_ThrowsKeyNotFound()
    {
        var f = BuildFixture();
        var request = new CreateTreatmentRequest
        {
            ClientId = f.ClientB.Id,
            TreatmentTypeId = f.TypeA.Id,
            TreatmentDate = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => f.TreatmentService.CreateAsync(CompanyA, request));
    }

    [Fact]
    public async Task TreatmentService_CreateAsync_TreatmentTypeFromOtherCompany_ThrowsKeyNotFound()
    {
        var f = BuildFixture();
        var request = new CreateTreatmentRequest
        {
            ClientId = f.ClientA.Id,
            TreatmentTypeId = f.TypeB.Id,
            TreatmentDate = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => f.TreatmentService.CreateAsync(CompanyA, request));
    }

    [Fact]
    public async Task TreatmentService_UpdateAsync_ReassignToOtherCompanyClient_ThrowsKeyNotFound()
    {
        var f = BuildFixture();
        var request = new UpdateTreatmentRequest
        {
            ClientId = f.ClientB.Id,
            TreatmentTypeId = f.TreatmentA.TreatmentTypeId,
            TreatmentDate = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => f.TreatmentService.UpdateAsync(f.TreatmentA.Id, request));
    }

    [Fact]
    public async Task TreatmentService_UpdateAsync_ReassignToOtherCompanyTreatmentType_ThrowsKeyNotFound()
    {
        var f = BuildFixture();
        var request = new UpdateTreatmentRequest
        {
            ClientId = f.TreatmentA.ClientId,
            TreatmentTypeId = f.TypeB.Id,
            TreatmentDate = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => f.TreatmentService.UpdateAsync(f.TreatmentA.Id, request));
    }

    [Fact]
    public async Task TreatmentService_UpdateAsync_SameCompanyReferences_Succeeds()
    {
        var f = BuildFixture();
        var request = new UpdateTreatmentRequest
        {
            ClientId = f.TreatmentA.ClientId,
            TreatmentTypeId = f.TreatmentA.TreatmentTypeId,
            TreatmentDate = DateTime.UtcNow,
            Notes = "updated"
        };

        var result = await f.TreatmentService.UpdateAsync(f.TreatmentA.Id, request);

        Assert.NotNull(result);
        Assert.Equal("updated", result!.Notes);
    }
}
