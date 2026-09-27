using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechServe.Web.Services;

namespace TechServe.Web.Controllers;

[Authorize(Roles = "ADMIN,MANAGER,STAFF,BILLING")]
public sealed class BillingController : Controller
{
    private readonly BillingService billingService;

    public BillingController(BillingService billingService)
    {
        this.billingService = billingService;
    }

    [HttpGet("/api/billing")]
    public async Task<IActionResult> Invoices(CancellationToken cancellationToken)
    {
        return Ok(await billingService.GetInvoicesAsync(cancellationToken));
    }

    [HttpPost("/api/billing/{invoiceId:int}/payments")]
    public async Task<IActionResult> RecordPayment(int invoiceId, [FromBody] PaymentInput input, CancellationToken cancellationToken)
    {
        var result = await billingService.RecordPaymentAsync(invoiceId, input.Amount, input.PaymentMethod, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(new { error = result.Error });
    }
}