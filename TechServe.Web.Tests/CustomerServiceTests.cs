using TechServe.Web.Services;
using Xunit;

namespace TechServe.Web.Tests;

public sealed class CustomerServiceTests
{
    [Fact]
    public async Task GetCustomersAsync_IncludesKnownCustomers()
    {
        var service = new CustomerService();

        var customers = await service.GetCustomersAsync();

        Assert.Contains(customers, c => c.Name == "Priya Nair");
        Assert.Contains(customers, c => c.Name == "Marcus Reed");
        Assert.Contains(customers, c => c.Name == "Jordan Lee");
    }
}
