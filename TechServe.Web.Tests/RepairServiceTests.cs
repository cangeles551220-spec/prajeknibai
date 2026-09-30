using Microsoft.EntityFrameworkCore;
using TechServe.Web.Data;
using TechServe.Web.Models;
using TechServe.Web.Services;
using Xunit;

namespace TechServe.Web.Tests;

public sealed class RepairServiceTests
{
    [Fact]
    public async Task GetTicketsAsync_IncludesKnownRepairJobs()
    {
        var service = new RepairService();

        var tickets = await service.GetTicketsAsync();

        Assert.Contains(tickets, t => t.JobId == "RJ-2024-001");
        Assert.Contains(tickets, t => t.JobId == "RJ-2024-002");
        Assert.Contains(tickets, t => t.JobId == "RJ-2024-004");
    }

    [Fact]
    public async Task CreateRepairAsync_UsesPendingStatus()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);
        var customer = new Customer
        {
            FullName = "Test Customer",
            ContactNumber = "09170000111",
            Email = "testcustomer@example.com",
            CustomerStatus = "Active",
            CreatedAt = DateTime.UtcNow
        };
        var device = new Device
        {
            Customer = customer,
            DeviceType = "Laptop",
            Brand = "Dell",
            Model = "Latitude 5440",
            SerialNumber = "SN-TEST-001",
            OperatingSystem = "Windows 11",
            DeviceCondition = "Good"
        };
        db.Customers.Add(customer);
        db.Devices.Add(device);
        await db.SaveChangesAsync();

        var service = new RepairService(db);
        var result = await service.CreateRepairAsync(new CreateRepairJobRequest
        {
            CustomerId = customer.CustomerId,
            DeviceId = device.DeviceId,
            Complaint = "Battery drains fast",
            Priority = "Normal"
        }, null);

        Assert.True(result.Success);
        Assert.Equal("Pending", result.Repair!.Status);
    }
}
