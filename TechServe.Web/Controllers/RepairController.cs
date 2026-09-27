using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TechServe.Web.Models;
using TechServe.Web.Services;

namespace TechServe.Web.Controllers;

public sealed class RepairController : Controller
{
    private readonly RepairService repairService;

    public RepairController(RepairService repairService)
    {
        this.repairService = repairService;
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,MANAGER,TECHNICIAN,STAFF,INVENTORY,BILLING")]
    public async Task<IActionResult> Index()
    {
        var repairs = await repairService.GetTrackingAsync(HttpContext.RequestAborted);
        if (User.IsInRole("TECHNICIAN"))
        {
            repairs = repairs.Where(repair => string.Equals(repair.Technician, User.Identity?.Name, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var viewModel = repairs
            .Select(repair => new RepairViewModel
            {
                JobId = repair.JobId,
                Customer = repair.Customer,
                Device = repair.Device,
                Status = repair.Status
            })
            .ToList();

        return View(viewModel);
    }

    [HttpGet("/api/repair-tracking")]
    [Authorize(Roles = "ADMIN,MANAGER,TECHNICIAN,STAFF,INVENTORY,BILLING")]
    public async Task<IActionResult> Tracking(CancellationToken cancellationToken)
    {
        var repairs = await repairService.GetTrackingAsync(cancellationToken);
        if (User.IsInRole("TECHNICIAN"))
        {
            repairs = repairs.Where(repair => string.Equals(repair.Technician, User.Identity?.Name, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return Ok(repairs);
    }

    [HttpGet("/api/repair-options")]
    [Authorize(Roles = "ADMIN,MANAGER,STAFF")]
    public async Task<IActionResult> RepairOptions(CancellationToken cancellationToken)
    {
        var options = await repairService.GetRepairOptionsAsync(cancellationToken);
        return Ok(User.IsInRole("ADMIN") || User.IsInRole("MANAGER")
            ? options
            : options with { Technicians = [] });
    }

    [HttpPost("/api/repair-jobs")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,MANAGER,STAFF")]
    public async Task<IActionResult> Create([FromBody] CreateRepairJobRequest request, CancellationToken cancellationToken)
    {
        if (User.IsInRole("STAFF") && request.TechnicianId.HasValue)
        {
            return Forbid();
        }
        var result = await repairService.CreateRepairAsync(
            request,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            cancellationToken);
        return result.Success
            ? Created("/api/repair-tracking", result.Repair)
            : BadRequest(new { error = result.Error });
    }

    [HttpPost("/api/repair-jobs/{id:int}/technician")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,MANAGER")]
    public async Task<IActionResult> AssignTechnician(int id, [FromBody] AssignRepairTechnicianRequest request, CancellationToken cancellationToken)
    {
        var result = await repairService.AssignTechnicianAsync(
            id,
            request.TechnicianId,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            cancellationToken);
        return result.Success
            ? Ok(result.Repair)
            : BadRequest(new { error = result.Error });
    }

    [HttpGet("/api/repair-parts/available")]
    [Authorize(Roles = "ADMIN,MANAGER,TECHNICIAN,STAFF,INVENTORY,BILLING")]
    public async Task<IActionResult> AvailableParts(CancellationToken cancellationToken)
    {
        return Ok(await repairService.GetAvailablePartsAsync(cancellationToken));
    }

    [HttpPost("/api/repair-tracking/{id:int}/status")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,MANAGER,TECHNICIAN,STAFF")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateRepairStatusViewModel model, CancellationToken cancellationToken)
    {
        var assignedTechnician = User.IsInRole("TECHNICIAN") ? User.Identity?.Name : null;
        var result = await repairService.UpdateWorkAsync(
            id,
            model,
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            assignedTechnician,
            cancellationToken);
        if (!result.Success && result.Error == "This repair is not assigned to you.")
        {
            return Forbid();
        }

        return result.Success ? Ok(result.Repair) : BadRequest(new { error = result.Error });
    }
}
