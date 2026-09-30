using TechServe.Web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace TechServe.Web.Tests;

public sealed class UserServiceTests
{
    [Fact]
    public async Task ValidateCredentialsAsync_AllowsKnownAdminCredentials()
    {
        var service = new UserService();

        var result = await service.ValidateCredentialsAsync("admin", "admin123");

        Assert.True(result);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_RejectsInvalidPassword()
    {
        var service = new UserService();

        var result = await service.ValidateCredentialsAsync("admin", "wrongpass");

        Assert.False(result);
    }

    [Fact]
    public async Task AuthenticateAsync_AllowsKnownAdminWhenDatabaseIsUnavailable()
    {
        var service = new UserService();

        var result = await service.AuthenticateAsync("admin", "admin123");

        Assert.NotNull(result);
        Assert.Equal("ADMIN", result.Role);
    }

    [Fact]
    public async Task InviteUserAsync_RequiresDatabase()
    {
        var service = new UserService();

        var result = await service.InviteUserAsync("New User", "newuser@example.com", "STAFF");

        Assert.False(result.Success);
        Assert.Null(result.Token);
    }

    [Fact]
    public async Task InviteUserAsync_RejectsUnknownRole()
    {
        var service = new UserService();

        var result = await service.InviteUserAsync("New User", "newuser@example.com", "SUPERADMIN");

        Assert.False(result.Success);
        Assert.Null(result.Token);
    }

    [Fact]
    public async Task ResetPasswordAsync_UpdatesPasswordForExistingUser()
    {
        var service = new UserService();

        var before = await service.AuthenticateAsync("admin", "admin123");
        Assert.NotNull(before);

        var result = await service.ResetPasswordAsync("admin", "NewAdmin456");

        Assert.True(result);
        var after = await service.AuthenticateAsync("admin", "NewAdmin456");
        Assert.NotNull(after);
        var old = await service.AuthenticateAsync("admin", "admin123");
        Assert.Null(old);
    }

    [Fact]
    public async Task ChangePasswordAsync_RequiresCurrentPasswordAndUpdatesUser()
    {
        var service = new UserService();

        var changed = await service.ChangePasswordAsync("admin", "admin123", "Admin456");

        Assert.True(changed);

        var valid = await service.AuthenticateAsync("admin", "Admin456");
        Assert.NotNull(valid);

        var oldPassword = await service.AuthenticateAsync("admin", "admin123");
        Assert.Null(oldPassword);

        await service.ResetPasswordAsync("admin", "admin123");
    }

    [Fact]
    public async Task UpdateProfileNameAsync_UpdatesKnownUserName()
    {
        var service = new UserService();

        var updated = await service.UpdateProfileNameAsync("admin", "Presentation Admin");

        Assert.True(updated);
        var authenticated = await service.AuthenticateAsync("admin", "admin123");
        Assert.NotNull(authenticated);
        Assert.Equal("Presentation Admin", authenticated.FullName);
    }

    [Fact]
    public async Task RequestPasswordResetAsync_DoesNotIssueCodeWithoutEmailConfiguration()
    {
        var sender = new SmtpEmailSender(new ConfigurationBuilder().Build(), NullLogger<SmtpEmailSender>.Instance);
        var service = new UserService(emailSender: sender);

        var requested = await service.RequestPasswordResetAsync("admin");
        var reset = await service.ResetPasswordAsync("admin", "123456", "NewPassword123");

        Assert.False(requested);
        Assert.False(reset);
    }

    [Fact]
    public async Task GetRoleDisplayNameAsync_ReturnsFriendlyName()
    {
        var service = new UserService();

        var result = await service.GetRoleDisplayNameAsync("ADMIN");

        Assert.Equal("Administrator", result);
    }
}
