using TechServe.Web.Services;
using Xunit;

namespace TechServe.Web.Tests;

public sealed class SalesServiceTests
{
    [Fact]
    public async Task GetTotalRevenueAsync_ReturnsExpectedRevenue()
    {
        var service = new SalesService();

        var revenue = await service.GetTotalRevenueAsync();

        Assert.Equal(24680m, revenue);
    }

    [Fact]
    public async Task GetSalesCountAsync_ReturnsExpectedCount()
    {
        var service = new SalesService();

        var count = await service.GetSalesCountAsync();

        Assert.Equal(126, count);
    }
}
