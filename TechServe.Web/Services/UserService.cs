using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace TechServe.Web.Services;

public sealed class UserService
{
    private static readonly HashSet<string> managedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "MANAGER", "TECHNICIAN", "STAFF", "INVENTORY", "BILLING"
    };
    private static readonly ConcurrentDictionary<string, (string Code, DateTime ExpiresAt)> resetCodes = new(StringComparer.OrdinalIgnoreCase);
    private readonly TechServeDatabase? database;
    private readonly PasswordHasher<string> passwordHasher = new();
    private readonly Dictionary<string, (string Password, string Role, string Name)> knownUsers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["admin"] = ("Admin123", "ADMIN", "System Administrator"),
        ["manager"] = ("Manager123", "MANAGER", "Manager / Supervisor"),
        ["employee"] = ("Employee123", "STAFF", "Employee / Service Staff"),
        ["technician"] = ("Technician@123", "TECHNICIAN", "Technician"),
        ["inventory"] = ("Inventory123", "INVENTORY", "Inventory Staff"),
        ["billing"] = ("Billing123", "BILLING", "Billing Staff")
    };

    public UserService(TechServeDatabase? database = null)
    {
        this.database = database;
    }

    public async Task<bool> ValidateCredentialsAsync(string? username, string? password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        if (database is null && knownUsers.TryGetValue(username.Trim(), out var fallbackUser))
        {
            return string.Equals(password.Trim(), fallbackUser.Password, StringComparison.OrdinalIgnoreCase);
        }

        var user = await FindUserAsync(username.Trim(), CancellationToken.None);
        return user is not null && user.IsActive && string.IsNullOrEmpty(user.InvitationTokenHash) && passwordHasher.VerifyHashedPassword(user.Username, user.PasswordHash, password.Trim()) != PasswordVerificationResult.Failed;
    }

    public async Task<AuthenticatedUser?> AuthenticateAsync(string? username, string? password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var normalizedUsername = username.Trim();
        var normalizedPassword = password.Trim();

        if (database is null && knownUsers.TryGetValue(normalizedUsername, out var localUser) &&
            string.Equals(normalizedPassword, localUser.Password, StringComparison.OrdinalIgnoreCase))
        {
            return new AuthenticatedUser(normalizedUsername, localUser.Name, localUser.Role);
        }

        UserRecord? user = null;

        try
        {
            user = await FindUserAsync(normalizedUsername, cancellationToken);
        }
        catch (SqlException) when (database is not null)
        {
            // Continue with the local demo accounts when LocalDB is unavailable.
        }

        if (user is not null)
        {
            if (user.IsActive && string.IsNullOrEmpty(user.InvitationTokenHash) && passwordHasher.VerifyHashedPassword(user.Username, user.PasswordHash, normalizedPassword) != PasswordVerificationResult.Failed)
            {
                return new AuthenticatedUser(user.Username, user.FullName, user.Role);
            }
        }

        if (!knownUsers.TryGetValue(normalizedUsername, out var fallbackUser) ||
            !string.Equals(normalizedPassword, fallbackUser.Password, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return new AuthenticatedUser(normalizedUsername, fallbackUser.Name, fallbackUser.Role);
    }

    public async Task EnsureSeedUsersAsync(CancellationToken cancellationToken = default)
    {
        if (database is null)
        {
            return;
        }

        var users = knownUsers.Select(user => new SeedUserRecord(
            user.Value.Name,
            user.Key,
            passwordHasher.HashPassword(user.Key, user.Value.Password),
            user.Value.Role)).ToArray();
        await database.UpsertSeedUsersAsync(users, cancellationToken);
    }

    public async Task<(bool Success, string? Token)> InviteUserAsync(string? fullName, string? email, string? role, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
        {
            return (false, null);
        }

        var normalizedFullName = fullName.Trim();
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var normalizedRole = string.IsNullOrWhiteSpace(role) ? "STAFF" : role.Trim().ToUpperInvariant();

        if (!managedRoles.Contains(normalizedRole) || knownUsers.ContainsKey(normalizedEmail))
        {
            return (false, null);
        }

        if (database is not null)
        {
            if (await FindUserAsync(normalizedEmail, cancellationToken) is not null)
            {
                return (false, null);
            }

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
            await database.AddInvitedUserAsync(
                normalizedEmail,
                normalizedEmail,
                normalizedFullName,
                passwordHasher.HashPassword(normalizedEmail, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))),
                normalizedRole,
                tokenHash,
                DateTime.UtcNow.AddHours(48),
                cancellationToken);
            return (true, token);
        }

        return (false, null);
    }

    public Task<IReadOnlyList<UserRecord>> GetUsersAsync(CancellationToken cancellationToken = default) =>
        database?.GetUsersAsync(cancellationToken) ?? Task.FromResult<IReadOnlyList<UserRecord>>(Array.Empty<UserRecord>());

    public Task<bool> DeactivateUserAsync(int userId, CancellationToken cancellationToken = default) =>
        database?.DeactivateUserAsync(userId, cancellationToken) ?? Task.FromResult(false);

    public async Task<bool> CompleteInvitationAsync(string? token, string? password, CancellationToken cancellationToken = default)
    {
        if (database is null || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(password) || password.Trim().Length < 8)
        {
            return false;
        }

        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim())));
        return await database.CompleteInvitationAsync(tokenHash, passwordHasher.HashPassword(token.Trim(), password.Trim()), cancellationToken);
    }

    public async Task<bool> RequestPasswordResetAsync(string? email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (await FindUserAsync(normalizedEmail, cancellationToken) is null && !knownUsers.ContainsKey(normalizedEmail))
        {
            return false;
        }

        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        resetCodes[normalizedEmail] = (code, DateTime.UtcNow.AddMinutes(15));
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string? email, string? newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(newPassword))
        {
            return false;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (database is not null)
        {
            if (await FindUserAsync(normalizedEmail, cancellationToken) is null)
            {
                return false;
            }

            await database.UpdatePasswordAsync(
                normalizedEmail,
                passwordHasher.HashPassword(normalizedEmail, newPassword.Trim()),
                cancellationToken);
            return true;
        }

        if (!knownUsers.TryGetValue(normalizedEmail, out var existingUser))
        {
            return false;
        }

        knownUsers[normalizedEmail] = (newPassword.Trim(), existingUser.Role, existingUser.Name);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string? email, string? code, string? newPassword, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(newPassword))
        {
            return false;
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (!resetCodes.TryGetValue(normalizedEmail, out var reset) || reset.ExpiresAt < DateTime.UtcNow || reset.Code != code.Trim())
        {
            return false;
        }

        var normalizedPassword = newPassword.Trim();

        if (database is not null)
        {
            if (await FindUserAsync(normalizedEmail, cancellationToken) is null)
            {
                return false;
            }

            await database.UpdatePasswordAsync(
                normalizedEmail,
                passwordHasher.HashPassword(normalizedEmail, normalizedPassword),
                cancellationToken);
            resetCodes.TryRemove(normalizedEmail, out _);
            return true;
        }

        if (!knownUsers.TryGetValue(normalizedEmail, out var existingUser))
        {
            return false;
        }

        knownUsers[normalizedEmail] = (normalizedPassword, existingUser.Role, existingUser.Name);
        resetCodes.TryRemove(normalizedEmail, out _);
        return true;
    }

    private async Task<UserRecord?> FindUserAsync(string username, CancellationToken cancellationToken)
    {
        if (database is not null)
        {
            return await database.FindUserAsync(username, cancellationToken);
        }

        return knownUsers.TryGetValue(username, out var user)
            ? new UserRecord(0, user.Name, username, username, passwordHasher.HashPassword(username, user.Password), user.Role)
            : null;
    }

    public Task<string> GetRoleDisplayNameAsync(string? role)
    {
        return Task.FromResult(role?.Trim() switch
        {
            "ADMIN" => "Administrator",
            "MANAGER" => "Manager",
            "TECHNICIAN" => "Technician",
            "STAFF" => "Staff",
            "INVENTORY" => "Inventory",
            "BILLING" => "Billing",
            _ => "User"
        });
    }
}

public sealed record AuthenticatedUser(string Username, string FullName, string Role);
