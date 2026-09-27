using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;

namespace prajeknibai.controller
{
    internal sealed class AppUser
    {
        public string FullName { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
    }

    internal static class UserRepository
    {
        private const int Pbkdf2Iterations = 100_000;
        private const int Pbkdf2SaltSize = 16;
        private const int Pbkdf2HashSize = 32;

        public static IReadOnlyList<AppUser> GetUsers()
        {
            EnsureDatabase();

            var users = new List<AppUser>();

            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand("SELECT FullName, Username, Role FROM Users ORDER BY FullName;", connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                users.Add(new AppUser
                {
                    FullName = reader.GetString(0),
                    Username = reader.GetString(1),
                    Role = reader.GetString(2)
                });
            }

            return users;
        }

        public static AppUser? AuthenticateAdmin(string login, string password)
        {
            return AuthenticateUser(login, password, role =>
                string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Super Admin", StringComparison.OrdinalIgnoreCase));
        }

        public static AppUser? AuthenticateStaff(string login, string password)
        {
            return AuthenticateUser(login, password, role =>
                !string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(role, "Super Admin", StringComparison.OrdinalIgnoreCase));
        }

        public static bool AddUser(string fullName, string username, string role, string password)
        {
            EnsureDatabase();

            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(@"
INSERT INTO Users (FullName, Username, Role, PasswordHash)
VALUES (@FullName, @Username, @Role, @PasswordHash);", connection);

            command.Parameters.AddWithValue("@FullName", fullName);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Role", role);
            command.Parameters.AddWithValue("@PasswordHash", HashPassword(password));

            try
            {
                return command.ExecuteNonQuery() == 1;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                return false;
            }
        }

        private static void EnsureDatabase()
        {
            EnsureDatabaseExists();
            EnsureUsersTableExists();
            EnsureExistingUsersHavePasswords();
            SeedDefaultUsers();
        }

        private static void EnsureDatabaseExists()
        {
            AppDatabase.EnsureDatabaseExists();
        }

        private static void EnsureUsersTableExists()
        {
            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(@"
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FullName NVARCHAR(100) NOT NULL,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        Role NVARCHAR(50) NOT NULL,
        PasswordHash NVARCHAR(512) NULL
    );
END;

IF COL_LENGTH(N'dbo.Users', N'PasswordHash') IS NULL
BEGIN
    ALTER TABLE dbo.Users
    ADD PasswordHash NVARCHAR(512) NULL;
END;

ALTER TABLE dbo.Users
ALTER COLUMN PasswordHash NVARCHAR(512) NULL;", connection);

            command.ExecuteNonQuery();
        }

        private static void EnsureExistingUsersHavePasswords()
        {
            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var selectCommand = new SqlCommand(@"
SELECT Username
FROM dbo.Users
WHERE PasswordHash IS NULL OR LTRIM(RTRIM(PasswordHash)) = N'';", connection);

            var usernames = new List<string>();
            using (var reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    usernames.Add(reader.GetString(0));
                }
            }

            foreach (var username in usernames)
            {
                using var updateCommand = new SqlCommand(@"
UPDATE dbo.Users
SET PasswordHash = @PasswordHash
WHERE Username = @Username;", connection);

                updateCommand.Parameters.AddWithValue("@Username", username);
                updateCommand.Parameters.AddWithValue("@PasswordHash", HashPassword(username));
                updateCommand.ExecuteNonQuery();
            }
        }

        private static AppUser? AuthenticateUser(string login, string password, Func<string, bool> rolePredicate)
        {
            EnsureDatabase();

            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(@"
SELECT FullName, Username, Role, PasswordHash
FROM dbo.Users
WHERE Username = @Login OR FullName = @Login;", connection);

            command.Parameters.AddWithValue("@Login", login);
            using var reader = command.ExecuteReader();

            AppUser? authenticatedUser = null;
            string? usernameToUpgrade = null;

            while (reader.Read())
            {
                var role = reader.GetString(2);
                var storedPasswordHash = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                if (!rolePredicate(role) || !VerifyPassword(password, storedPasswordHash))
                {
                    continue;
                }

                authenticatedUser = new AppUser
                {
                    FullName = reader.GetString(0),
                    Username = reader.GetString(1),
                    Role = role
                };

                if (IsLegacyPasswordHash(storedPasswordHash))
                {
                    usernameToUpgrade = authenticatedUser.Username;
                }

                break;
            }

            if (authenticatedUser != null && !string.IsNullOrWhiteSpace(usernameToUpgrade))
            {
                UpgradePasswordHash(usernameToUpgrade, password);
            }

            return authenticatedUser;
        }

        private static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(Pbkdf2SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256, Pbkdf2HashSize);
            return string.Concat(
                "PBKDF2$",
                Pbkdf2Iterations.ToString(),
                "$",
                Convert.ToHexString(salt),
                "$",
                Convert.ToHexString(hash));
        }

        private static bool VerifyPassword(string password, string storedPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(storedPasswordHash))
            {
                return false;
            }

            if (IsLegacyPasswordHash(storedPasswordHash))
            {
                using var sha256 = SHA256.Create();
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return string.Equals(Convert.ToHexString(bytes), storedPasswordHash, StringComparison.OrdinalIgnoreCase);
            }

            var parts = storedPasswordHash.Split('$');
            if (parts.Length != 4 || !string.Equals(parts[0], "PBKDF2", StringComparison.Ordinal))
            {
                return false;
            }

            if (!int.TryParse(parts[1], out var iterations))
            {
                return false;
            }

            var salt = Convert.FromHexString(parts[2]);
            var expectedHash = Convert.FromHexString(parts[3]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }

        private static bool IsLegacyPasswordHash(string storedPasswordHash)
        {
            return !string.IsNullOrWhiteSpace(storedPasswordHash)
                && storedPasswordHash.Length == 64
                && storedPasswordHash.IndexOf('$') < 0;
        }

        private static void UpgradePasswordHash(string username, string password)
        {
            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(@"
UPDATE dbo.Users
SET PasswordHash = @PasswordHash
WHERE Username = @Username;", connection);

            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@PasswordHash", HashPassword(password));
            command.ExecuteNonQuery();
        }

        private static void SeedUserIfMissing(SqlConnection connection, string fullName, string username, string role, string password)
        {
            using var command = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = @Username)
BEGIN
    INSERT INTO dbo.Users (FullName, Username, Role, PasswordHash)
    VALUES (@FullName, @Username, @Role, @PasswordHash);
END;", connection);

            command.Parameters.AddWithValue("@FullName", fullName);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Role", role);
            command.Parameters.AddWithValue("@PasswordHash", HashPassword(password));
            command.ExecuteNonQuery();
        }

        private static void SeedDefaultUsers()
        {
            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            SeedUserIfMissing(connection, "System Administrator", "admin", "Admin", "Admin123");
            SeedUserIfMissing(connection, "Super Admin", "superadmin", "Super Admin", "superadmin");
            SeedUserIfMissing(connection, "John", "john", "Admin", "john");
            SeedUserIfMissing(connection, "Jane", "jane", "Manager", "jane");
            SeedUserIfMissing(connection, "Bob", "bob", "Staff", "bob");
        }
    }
}
