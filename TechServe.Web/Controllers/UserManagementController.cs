using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechServe.Web.Models;
using TechServe.Web.Services;

namespace TechServe.Web.Controllers;

[Authorize(Roles = "ADMIN")]
public sealed class UserManagementController : Controller
{
    private static readonly HashSet<string> allowedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "MANAGER", "TECHNICIAN", "STAFF", "INVENTORY", "BILLING"
    };
    private readonly UserService userService;

    public UserManagementController(UserService userService)
    {
        this.userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return RedirectToAction("UserManagement", "Admin");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Invite(UserManagementViewModel model, string? returnUrl, CancellationToken cancellationToken)
    {
        var managementReturn = IsUserManagementReturn(returnUrl);
        if (!allowedRoles.Contains(model.Invite.Role))
        {
            ModelState.AddModelError("Invite.Role", "That role cannot be assigned by this form.");
        }

        if (!ModelState.IsValid)
        {
            if (managementReturn)
            {
                TempData["UserManagementError"] = "Please provide a valid name, email, and allowed role.";
                return RedirectToAction(nameof(Index));
            }
            TempData["UserManagementError"] = "Please provide a valid name, email, and allowed role.";
            return RedirectToAction("UserManagement", "Admin");
        }

        var result = await userService.InviteUserAsync(model.Invite.FullName, model.Invite.Email, model.Invite.Role, cancellationToken);
        if (!result.Success || result.Token is null)
        {
            if (managementReturn)
            {
                TempData["UserManagementError"] = "That email is already registered or the account could not be created.";
                return RedirectToAction(nameof(Index));
            }
            TempData["UserManagementError"] = "That email is already registered or the account could not be created.";
            return RedirectToAction("UserManagement", "Admin");
        }

        var invitationUrl = Url.Action("SetPassword", "Auth", new { token = result.Token }, Request.Scheme);
        if (managementReturn)
        {
            TempData["InvitationUrl"] = invitationUrl;
            return RedirectToAction(nameof(Index));
        }
        TempData["InvitationUrl"] = invitationUrl;
        return RedirectToAction("UserManagement", "Admin");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id, string? returnUrl, CancellationToken cancellationToken)
    {
        await userService.DeactivateUserAsync(id, cancellationToken);
        if (IsUserManagementReturn(returnUrl))
        {
            return RedirectToAction(nameof(Index));
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? returnUrl, CancellationToken cancellationToken)
    {
        await userService.DeleteUserAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private bool IsUserManagementReturn(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) &&
        (returnUrl.StartsWith("/Admin/UserManagement", StringComparison.OrdinalIgnoreCase) ||
         returnUrl.StartsWith("/UserManagement/Index", StringComparison.OrdinalIgnoreCase));
}