using ACT.Application.Common;
using ACT.Application.Services;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Infrastructure.Persistence;
using ACT.Infrastructure.Repositories;
using ACT.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace ACT.Tests;

/// <summary>
/// B3 — page/pageSize must be clamped to sane bounds, and the clamped values must be what the
/// response metadata (Page/PageSize/TotalPages) actually describes, not the caller's raw input —
/// otherwise Items reflects one page size while the metadata claims another.
/// </summary>
public class PagingTests
{
    [Theory]
    [InlineData(1, 20, 1, 20)]
    [InlineData(0, 20, 1, 20)]
    [InlineData(-5, 20, 1, 20)]
    [InlineData(1, 999_999_999, 1, 100)]
    [InlineData(1, 0, 1, 1)]
    [InlineData(1, -10, 1, 1)]
    public void Clamp_BoundsPageAndPageSize(int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        var (clampedPage, clampedPageSize) = Paging.Clamp(page, pageSize);

        Assert.Equal(expectedPage, clampedPage);
        Assert.Equal(expectedPageSize, clampedPageSize);
    }

    private class SystemTenantContext : ITenantContext
    {
        public int? CompanyId => null;
        public bool IsSuperAdmin => true;
    }

    [Fact]
    public async Task ClientService_GetPagedAsync_HugePageSize_ResultMetadataReflectsClampedValue()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new AppDbContext(options, new SystemTenantContext());
        for (var i = 1; i <= 5; i++)
            context.Clients.Add(new Client { Id = i, FirstName = $"C{i}", LastName = "Test", CompanyId = 1 });
        await context.SaveChangesAsync();

        var service = new ClientService(new ClientRepository(context), new NoopAuditService());
        var result = await service.GetPagedAsync(companyId: 1, page: 1, pageSize: 999_999_999);

        // Metadata must match what was actually queried, not the caller's raw request.
        Assert.Equal(100, result.PageSize);
        Assert.Equal(1, result.Page);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(5, result.Items.Count());
    }
}
