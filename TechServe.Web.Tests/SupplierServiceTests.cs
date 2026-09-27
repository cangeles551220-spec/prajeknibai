using TechServe.Web.Services;
using Xunit;

namespace TechServe.Web.Tests;

public sealed class SupplierServiceTests
{
    [Fact]
    public async Task GetSuppliersAsync_IncludesCoreVendors()
    {
        var service = new SupplierService();

        var suppliers = await service.GetSuppliersAsync();

        Assert.Contains(suppliers, s => s.Name == "TechSource Supply");
        Assert.Contains(suppliers, s => s.Name == "PartsHub");
        Assert.Contains(suppliers, s => s.Name == "MobileFix Co.");
    }

    [Fact]
    public async Task AddSupplierAsync_CreatesSupplierWithDefaults()
    {
        var service = new SupplierService();

        var created = await service.AddSupplierAsync("Northwind Components", "Motherboards");

        Assert.Equal("Northwind Components", created.Name);
        Assert.Equal("Motherboards", created.Category);
        Assert.True(created.SupplierId > 0);
        Assert.Equal("Active", created.Status);
    }

    [Fact]
    public async Task UpdateSupplierAsync_UpdatesExistingSupplierDetails()
    {
        var service = new SupplierService();
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
}
