namespace TechServe.Web.Models;

public sealed class ForgotPasswordViewModel
{
    public string Email { get; set; } = string.Empty;
    public string ResetCode { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public bool CodeSent { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }
}
