using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using TechServe.Web.Models;
using TechServe.Web.Services;

namespace TechServe.Web.Controllers;

public sealed class AuthController : Controller
{
    private readonly UserService userService;

    public AuthController(UserService userService)
    {
        this.userService = userService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        var model = new LoginViewModel
        {
            SuccessMessage = TempData["SuccessMessage"] as string
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userService.AuthenticateAsync(model.Username, model.Password, HttpContext.RequestAborted);
        if (user is null)
        {
            model.ErrorMessage = "Invalid username or password.";
            return View(model);
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Username),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        };
        await HttpContext.SignInAsync("TechServeCookie", new ClaimsPrincipal(new ClaimsIdentity(claims, "TechServeCookie")));

        return user.Role switch
        {
            "ADMIN" or "MANAGER" or "TECHNICIAN" or "STAFF" or "INVENTORY" or "BILLING"
                => RedirectToAction("Dashboard", "Admin"),
            _ => RedirectToAction(nameof(AccessDenied))
        };
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    [HttpGet]
    public IActionResult SetPassword(string? token)
    {
        return View(new InvitationSetupViewModel { Token = token ?? string.Empty });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPassword(InvitationSetupViewModel model)
    {
        if (!ModelState.IsValid || !string.Equals(model.Password, model.ConfirmPassword, StringComparison.Ordinal))
        {
            if (!string.Equals(model.Password, model.ConfirmPassword, StringComparison.Ordinal))
            {
                model.ErrorMessage = "The passwords do not match.";
            }
            return View(model);
        }

        if (!await userService.CompleteInvitationAsync(model.Token, model.Password, HttpContext.RequestAborted))
        {
            model.ErrorMessage = "This invitation is invalid or expired.";
            return View(model);
        }

        TempData["SuccessMessage"] = "Password created. You can now sign in.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!model.CodeSent)
        {
            if (!await userService.RequestPasswordResetAsync(model.Email, HttpContext.RequestAborted))
            {
                model.ErrorMessage = "No account is registered with that email address.";
                return View(model);
            }

            model.CodeSent = true;
            model.SuccessMessage = "A password reset code was sent to your registered email address.";
            return View(model);
        }

        if (!string.Equals(model.NewPassword, model.ConfirmPassword, StringComparison.Ordinal))
        {
            model.ErrorMessage = "The new passwords do not match.";
            return View(model);
        }

        if (!await userService.ResetPasswordAsync(model.Email, model.ResetCode, model.NewPassword, HttpContext.RequestAborted))
        {
            model.CodeSent = true;
            model.ErrorMessage = "That reset code is invalid or expired. Request a new code and try again.";
            return View(model);
        }

        TempData["SuccessMessage"] = "Password reset successful. You can now sign in with your new password.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("TechServeCookie");
        return RedirectToAction(nameof(Login));
    }
}
