using Microsoft.AspNetCore.Mvc;

namespace TechServe.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
