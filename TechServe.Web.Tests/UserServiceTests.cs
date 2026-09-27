using TechServe.Web.Services;
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
    public async Task RegisterAsync_CreatesUserWithKnownPassword()
    {
        var service = new UserService();

        var result = await service.RegisterAsync("New User", "newuser", "Password123", "STAFF");

        Assert.True(result);
        var auth = await service.AuthenticateAsync("newuser", "Password123");
        Assert.NotNull(auth);
        Assert.Equal("STAFF", auth.Role);
    }

    [Fact]
    public async Task RegisterAsync_SavesSelectedRoleForLogin()
    {
        var service = new UserService();

        var result = await service.RegisterAsync(
            "Registered Manager",
            "registered.manager",
            "manager@example.com",
            "Password123",
            "MANAGER");

        Assert.True(result);
        var auth = await service.AuthenticateAsync("manager@example.com", "Password123");
        Assert.NotNull(auth);
        Assert.Equal("MANAGER", auth.Role);
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
    public async Task GetRoleDisplayNameAsync_ReturnsFriendlyName()
    {
        var service = new UserService();

        var result = await service.GetRoleDisplayNameAsync("ADMIN");

        Assert.Equal("Administrator", result);
    }
}
