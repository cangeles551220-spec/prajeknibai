using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
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
        await HttpContext.SignInAsync(
            "TechServeCookie",
            new ClaimsPrincipal(new ClaimsIdentity(claims, "TechServeCookie")),
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                AllowRefresh = true,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
            });

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

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { error = "Request body is required." });
        }

        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(username))
        {
            return Unauthorized();
        }

        var currentPassword = request.CurrentPassword?.Trim() ?? string.Empty;
        var newPassword = request.NewPassword?.Trim() ?? string.Empty;
        var confirmPassword = request.ConfirmPassword?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            return BadRequest(new { error = "Complete all password fields." });
        }

        if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
        {
            return BadRequest(new { error = "The new passwords do not match." });
        }

        if (newPassword.Length < 8)
        {
            return BadRequest(new { error = "The new password must be at least 8 characters long." });
        }

        if (string.Equals(newPassword, currentPassword, StringComparison.Ordinal))
        {
            return BadRequest(new { error = "The new password must be different from your current password." });
        }

        if (!await userService.ChangePasswordAsync(username, currentPassword, newPassword, HttpContext.RequestAborted))
        {
            return BadRequest(new { error = "Your current password is incorrect." });
        }

        return Ok(new { message = "Password changed successfully." });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var fullName = request?.FullName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(username) || fullName.Length < 2 || fullName.Length > 120)
        {
            return BadRequest(new { error = "Enter a name between 2 and 120 characters." });
        }

        if (!await userService.UpdateProfileNameAsync(username, fullName, HttpContext.RequestAborted))
        {
            return BadRequest(new { error = "Your profile could not be updated." });
        }

        var role = User.FindFirstValue(ClaimTypes.Role) ?? "STAFF";
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, username),
            new Claim(ClaimTypes.Name, fullName),
            new Claim(ClaimTypes.Role, role)
        };
        await HttpContext.SignInAsync("TechServeCookie", new ClaimsPrincipal(new ClaimsIdentity(claims, "TechServeCookie")));

        return Ok(new { fullName, message = "Profile updated successfully." });
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
                model.ErrorMessage = "We could not send a reset code. Check the email address or contact an administrator.";
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

    public sealed record ChangePasswordRequest(string? CurrentPassword, string? NewPassword, string? ConfirmPassword);
    public sealed record UpdateProfileRequest(string? FullName);
}
