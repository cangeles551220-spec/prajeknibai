using TechServe.Web.Services;
using Xunit;

namespace TechServe.Web.Tests;

public sealed class ProductServiceTests
{
    [Fact]
    public async Task GetProductsAsync_IncludesKeyInventoryItems()
    {
        var service = new ProductService();

        var products = await service.GetProductsAsync();

        Assert.Contains(products, p => p.Name == "Laptop");
        Assert.Contains(products, p => p.Name == "Monitor");
        Assert.Contains(products, p => p.Name == "Keyboard");
    }
}
