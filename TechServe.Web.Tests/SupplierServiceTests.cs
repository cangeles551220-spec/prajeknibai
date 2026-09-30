using Microsoft.EntityFrameworkCore;
using TechServe.Web.Data;
using TechServe.Web.Services;
using Xunit;

namespace TechServe.Web.Tests;

public sealed class SupplierServiceTests
{
    [Fact]
    public async Task GetSuppliersAsync_ReturnsEmptyWhenNoSuppliersExist()
    {
        using var database = CreateDatabase();
        var service = new SupplierService(database);

        var suppliers = await service.GetSuppliersAsync();

        Assert.Empty(suppliers);
    }

    [Fact]
    public async Task AddSupplierAsync_CreatesSupplierWithDefaults()
    {
        using var database = CreateDatabase();
        var service = new SupplierService(database);

        var created = await service.AddSupplierAsync("Northwind Components", "Motherboards");

        Assert.Equal("Northwind Components", created.Name);
        Assert.Equal("Motherboards", created.Category);
        Assert.True(created.SupplierId > 0);
        Assert.Equal("Active", created.Status);
    }

    [Fact]
    public async Task UpdateSupplierAsync_UpdatesExistingSupplierDetails()
    {
        using var database = CreateDatabase();
        var service = new SupplierService(database);
        var created = await service.AddSupplierAsync("Atlas Hardware", "Displays");

        var updated = await service.UpdateSupplierAsync(new SupplierItem
        {
            SupplierId = created.SupplierId,
            Name = "Atlas Hardware Ltd.",
            Category = "Displays",
            ContactEmail = "ops@atlashardware.example",
            Phone = "+1 (555) 999-0001",
            Status = "Inactive"
        });

        Assert.NotNull(updated);
        Assert.Equal("Atlas Hardware Ltd.", updated.Name);
        Assert.Equal("ops@atlashardware.example", updated.ContactEmail);
        Assert.Equal("Inactive", updated.Status);
    }

    private static ApplicationDbContext CreateDatabase() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
