using Microsoft.EntityFrameworkCore;
using TechServe.Web.Data;

namespace TechServe.Web.Services;

public sealed class ReportService
{
    private readonly ApplicationDbContext db;

    public ReportService(ApplicationDbContext db)
    {
        this.db = db;
    }

    public async Task<ReportSummary> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var invoices = await db.Invoices
            .Select(invoice => new { invoice.LaborCost, invoice.PartsCost, invoice.OtherCharges, invoice.Discount })
            .ToListAsync(cancellationToken);
        var completedJobs = await db.RepairJobs
            .Where(job => job.RepairStatus == "Completed" || job.CompletionDate != null)
            .Select(job => new
            {
                job.TechnicianId,
                TechnicianName = job.Technician == null ? "Unassigned" : job.Technician.FullName,
                job.Device.DeviceType,
                job.DateReceived,
                job.CompletionDate
            })
            .ToListAsync(cancellationToken);

        var technicians = completedJobs
            .GroupBy(job => new { job.TechnicianId, job.TechnicianName })
            .Select(group => new TechnicianReport(
                group.Key.TechnicianName,
                group.Count(),
                group.Average(job => Math.Max(0, (job.CompletionDate ?? DateTime.UtcNow).Subtract(job.DateReceived).TotalDays))))
            .OrderByDescending(item => item.Completed)
            .ToList();
        var deviceTotal = completedJobs.Count;
        var deviceTypes = completedJobs
            .GroupBy(job => job.DeviceType)
            .Select(group => new DeviceTypeReport(group.Key, group.Count(), deviceTotal == 0 ? 0 : group.Count() * 100m / deviceTotal))
            .OrderByDescending(item => item.Repairs)
            .ToList();

        return new ReportSummary(
            invoices.Sum(invoice => invoice.LaborCost + invoice.PartsCost + invoice.OtherCharges - invoice.Discount),
            completedJobs.Count,
            technicians,
            deviceTypes,
            DateTime.UtcNow);
    }
}

public sealed record ReportSummary(
    decimal Revenue,
    int CompletedRepairs,
    IReadOnlyList<TechnicianReport> Technicians,
    IReadOnlyList<DeviceTypeReport> DeviceTypes,
    DateTime GeneratedAt);

public sealed record TechnicianReport(string Name, int Completed, double AverageTurnaroundDays);

public sealed record DeviceTypeReport(string Name, int Repairs, decimal Percentage);
