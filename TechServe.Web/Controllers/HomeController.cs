using Microsoft.AspNetCore.Mvc;
using TechServe.Web.Models;

namespace TechServe.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        return View(new DashboardViewModel());
    }
}
