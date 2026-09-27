using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechServe.Web.Models;
using TechServe.Web.Services;

namespace TechServe.Web.Controllers;

public sealed class AdminController : Controller
{
    private readonly UserService userService;

    public AdminController(UserService userService)
    {
        this.userService = userService;
    }

    [Authorize(Roles = "ADMIN,MANAGER,TECHNICIAN,STAFF,INVENTORY,BILLING")]
    [HttpGet]
    public IActionResult Dashboard()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "STAFF";
        return View(new DashboardViewModel
        {
            WorkspaceName = "TechServe HQ",
            CurrentRole = role,
            DisplayName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                ?? User.Identity?.Name
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? "User"
        });
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet]
    public async Task<IActionResult> UserManagement(CancellationToken cancellationToken)
    {
        var users = (await userService.GetUsersAsync(cancellationToken))
            .Select(user => new UserSummary(user.Id, user.FullName, user.Email, user.Role, user.IsActive, !string.IsNullOrEmpty(user.InvitationTokenHash)))
            .ToArray();
        return View(new UserManagementViewModel
        {
            Users = users,
            InvitationUrl = TempData["InvitationUrl"] as string,
            ErrorMessage = TempData["UserManagementError"] as string
        });
    }
}
