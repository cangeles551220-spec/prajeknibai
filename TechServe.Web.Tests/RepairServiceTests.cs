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
}
