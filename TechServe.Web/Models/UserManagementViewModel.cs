using System.ComponentModel.DataAnnotations;

namespace TechServe.Web.Models;

public sealed class UserManagementViewModel
{
    public IReadOnlyList<UserSummary> Users { get; init; } = Array.Empty<UserSummary>();
    public InviteUserInput Invite { get; init; } = new();
    public string? InvitationUrl { get; init; }
    public string? ErrorMessage { get; init; }
    public bool ShowArchived { get; init; }
    public int TotalUsers => Users.Count;
    public int ActiveUsers => Users.Count(user => user.IsActive);
    public int Administrators => Users.Count(user => string.Equals(user.Role, "ADMIN", StringComparison.OrdinalIgnoreCase));
}

public sealed class InviteUserInput
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(180)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "STAFF";
}

public sealed record UserSummary(int Id, string FullName, string Email, string Role, bool IsActive, bool InvitationPending);

public sealed class InvitationSetupViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
}