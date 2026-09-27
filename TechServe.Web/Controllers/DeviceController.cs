using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechServe.Web.Models;
using TechServe.Web.Services;

namespace TechServe.Web.Controllers;

[Route("api/devices")]
[ApiController]
public sealed class DeviceController : ControllerBase
{
    private readonly DeviceService deviceService;

    public DeviceController(DeviceService deviceService)
    {
        this.deviceService = deviceService;
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,MANAGER,STAFF,TECHNICIAN,INVENTORY")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await deviceService.GetDevicesAsync(cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,MANAGER,STAFF")]
    public async Task<IActionResult> Create([FromBody] SaveDeviceRequest request, CancellationToken cancellationToken)
    {
        var result = await deviceService.CreateAsync(request, cancellationToken);
        return result.Success ? Created("/api/devices", result.Device) : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,MANAGER,STAFF")]
    public async Task<IActionResult> Update(int id, [FromBody] SaveDeviceRequest request, CancellationToken cancellationToken)
    {
        var result = await deviceService.UpdateAsync(id, request, cancellationToken);
        return result.Success ? Ok(result.Device) : BadRequest(new { error = result.Error });
    }
}