using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechServe.Web.Services;

namespace TechServe.Web.Controllers;

public sealed class ReportController : Controller
{
    private readonly ReportService reportService;

    public ReportController(ReportService reportService)
    {
        this.reportService = reportService;
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,MANAGER,INVENTORY,BILLING")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var summary = await reportService.GetSummaryAsync(cancellationToken);

        ViewBag.Revenue = summary.Revenue;
        ViewBag.CompletedRepairs = summary.CompletedRepairs;

        return View();
    }

    [HttpGet("/api/reports/summary")]
    [Authorize(Roles = "ADMIN,MANAGER,INVENTORY,BILLING")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        return Ok(await reportService.GetSummaryAsync(cancellationToken));
    }
}
