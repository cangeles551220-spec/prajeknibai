using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechServe.Web.Models;
using TechServe.Web.Services;

namespace TechServe.Web.Controllers;

public sealed class SupplierController : Controller
{
    private readonly SupplierService supplierService;

    public SupplierController(SupplierService supplierService)
    {
        this.supplierService = supplierService;
    }

    [HttpGet("/api/suppliers")]
    [Authorize(Roles = "ADMIN,MANAGER,INVENTORY")]
    public async Task<IActionResult> ListApi(CancellationToken cancellationToken)
    {
        return Ok(await supplierService.GetSuppliersAsync(cancellationToken));
    }

    [HttpPost("/api/suppliers")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,MANAGER,INVENTORY")]
    public async Task<IActionResult> CreateApi([FromBody] SupplierViewModel model, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Category))
        {
            return BadRequest(new { error = "Supplier name and category are required." });
        }

        var supplier = await supplierService.AddSupplierAsync(
            model.Name, model.Category, model.ContactPerson, model.ContactEmail, model.Phone,
            model.Address, model.Status, model.PaymentTerms, model.LeadTime, cancellationToken);
        return Created("/api/suppliers", supplier);
    }

    [HttpPut("/api/suppliers/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,MANAGER,INVENTORY")]
    public async Task<IActionResult> UpdateApi(int id, [FromBody] SupplierViewModel model, CancellationToken cancellationToken)
    {
        if (id <= 0 || string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Category))
        {
            return BadRequest(new { error = "Supplier name and category are required." });
        }

        model.SupplierId = id;
        var updated = await supplierService.UpdateSupplierAsync(new SupplierItem
        {
            SupplierId = id,
            Name = model.Name,
            Category = model.Category,
            ContactPerson = model.ContactPerson,
            ContactEmail = model.ContactEmail,
            Phone = model.Phone,
            Address = model.Address,
            Status = model.Status,
            PaymentTerms = model.PaymentTerms,
            LeadTime = model.LeadTime
        }, cancellationToken);
        return updated is null ? NotFound(new { error = "Supplier was not found." }) : Ok(updated);
    }

    [HttpPost("/api/suppliers/{id:int}/deactivate")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,MANAGER,INVENTORY")]
    public async Task<IActionResult> DeactivateApi(int id, CancellationToken cancellationToken)
    {
        return await supplierService.DeleteSupplierAsync(id, cancellationToken)
            ? NoContent()
            : NotFound(new { error = "Supplier was not found or is already inactive." });
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,MANAGER,INVENTORY")]
    public async Task<IActionResult> Index()
    {
        var suppliers = await supplierService.GetSuppliersAsync();
        var viewModel = suppliers
            .Select(s => new SupplierViewModel
            {
                SupplierId = s.SupplierId,
                Name = s.Name,
                Category = s.Category,
                ContactPerson = s.ContactPerson,
                ContactEmail = s.ContactEmail,
                Phone = s.Phone,
                Address = s.Address,
                Status = s.Status,
                PaymentTerms = s.PaymentTerms,
                LeadTime = s.LeadTime
            })
            .ToList();

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,MANAGER,INVENTORY")]
    public async Task<IActionResult> Create(SupplierViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Category))
        {
            TempData["SupplierError"] = "Supplier name and category are required.";
            return RedirectToAction(nameof(Index));
        }

        await supplierService.AddSupplierAsync(
            model.Name,
            model.Category,
            model.ContactPerson,
            model.ContactEmail,
            model.Phone,
            model.Address,
            model.Status,
            model.PaymentTerms,
            model.LeadTime);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,MANAGER,INVENTORY")]
    public async Task<IActionResult> Update(SupplierViewModel model)
    {
        if (model.SupplierId <= 0 || string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Category))
        {
            TempData["SupplierError"] = "Select a valid supplier to update.";
            return RedirectToAction(nameof(Index));
        }

        var updated = await supplierService.UpdateSupplierAsync(new SupplierItem
        {
            SupplierId = model.SupplierId,
            Name = model.Name,
            Category = model.Category,
            ContactPerson = model.ContactPerson,
            ContactEmail = model.ContactEmail,
            Phone = model.Phone,
            Address = model.Address,
            Status = model.Status,
            PaymentTerms = model.PaymentTerms,
            LeadTime = model.LeadTime
        });

        if (updated is null)
        {
            TempData["SupplierError"] = "Supplier could not be found for update.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "ADMIN,MANAGER,INVENTORY")]
    public async Task<IActionResult> Delete(int id)
    {
        await supplierService.DeleteSupplierAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
