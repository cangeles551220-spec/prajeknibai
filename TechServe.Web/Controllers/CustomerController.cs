using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechServe.Web.Models;
using TechServe.Web.Services;

namespace TechServe.Web.Controllers;

public sealed class CustomerController : Controller
{
    private readonly CustomerService customerService;

    public CustomerController(CustomerService customerService)
    {
        this.customerService = customerService;
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN,MANAGER,STAFF,BILLING")]
    public async Task<IActionResult> Index()
    {
        var customers = await customerService.GetCustomersAsync();
        var viewModel = customers
            .Select(c => new CustomerViewModel
            {
                Name = c.Name,
                CustomerId = c.CustomerId,
                Status = c.Status
            })
            .ToList();

        return View(viewModel);
    }
}
