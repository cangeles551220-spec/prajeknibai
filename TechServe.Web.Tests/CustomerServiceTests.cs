using Microsoft.EntityFrameworkCore;
using TechServe.Web.Data;
using TechServe.Web.Services;
using Xunit;

namespace TechServe.Web.Tests;

public sealed class CustomerServiceTests
{
    [Fact]
    public async Task GetCustomersAsync_ReturnsRealCustomersFromDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);
        db.Customers.Add(new Customer
        {
            FullName = "Alice Johnson",
            ContactNumber = "09170000000",
            Email = "alice@example.com",
            CustomerStatus = "Active",
            CreatedAt = DateTime.UtcNow
        });
        db.Customers.Add(new Customer
        {
            FullName = "Bob Smith",
            ContactNumber = "09170000001",
            Email = "bob@example.com",
            CustomerStatus = "Returning",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new CustomerService(db);

        var customers = await service.GetCustomersAsync();

        Assert.Contains(customers, c => c.Name == "Alice Johnson");
        Assert.Contains(customers, c => c.Name == "Bob Smith");
        Assert.DoesNotContain(customers, c => c.Name == "Priya Nair");
    }

    [Fact]
    public async Task AddCustomerAsync_RejectsDuplicateCustomer()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);
        db.Customers.Add(new Customer
        {
            FullName = "Maria Santos",
            ContactNumber = "09170000099",
            Email = "maria@example.com",
            CustomerStatus = "Active",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = new CustomerService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddCustomerAsync(
            "Maria Santos",
            "09170000099",
            "maria@example.com",
            "Davao City"));
    }
}
