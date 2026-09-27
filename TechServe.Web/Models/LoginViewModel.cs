namespace TechServe.Web.Models;

public sealed class LoginViewModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
}
