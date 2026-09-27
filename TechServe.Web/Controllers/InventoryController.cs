using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechServe.Web.Models;
using TechServe.Web.Services;

namespace TechServe.Web.Controllers;

[Route("api/inventory")]
[ApiController]
public sealed class InventoryController : ControllerBase
{
    private readonly InventoryService inventoryService;

    public InventoryController(InventoryService inventoryService)
    {
        this.inventoryService = inventoryService;
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,MANAGER,INVENTORY")]
    public async Task<IActionResult> GetParts(CancellationToken cancellationToken) =>
        Ok(await inventoryService.GetPartsAsync(cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,INVENTORY")]
    public async Task<IActionResult> CreatePart([FromBody] SaveInventoryPartRequest request, CancellationToken cancellationToken)
    {
        var result = await inventoryService.CreatePartAsync(request, cancellationToken);
        return result.Success ? Created("/api/inventory", result.Part) : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,INVENTORY")]
    public async Task<IActionResult> UpdatePart(int id, [FromBody] SaveInventoryPartRequest request, CancellationToken cancellationToken)
    {
        var result = await inventoryService.UpdatePartAsync(id, request, cancellationToken);
        return result.Success ? Ok(result.Part) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:int}/stock")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,MANAGER,INVENTORY")]
    public async Task<IActionResult> AdjustStock(int id, [FromBody] AdjustInventoryRequest request, CancellationToken cancellationToken)
    {
        var result = await inventoryService.AdjustStockAsync(id, request.Direction, request.Quantity, request.SupplierId, request.ReferenceNumber, cancellationToken);
        return result.Success ? Ok(result.Part) : BadRequest(new { error = result.Error });
    }
}