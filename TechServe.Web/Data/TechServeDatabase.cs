using Microsoft.Data.SqlClient;
using TechServe.Web.Models;

public sealed class TechServeDatabase
{
    private readonly string connectionString;
    private readonly string schemaPath;

    public TechServeDatabase(IConfiguration configuration, IWebHostEnvironment environment)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");
        schemaPath = Path.Combine(environment.ContentRootPath, "Data", "TechServe.Database.sql");
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
      
        if (!File.Exists(schemaPath))
        {
            throw new FileNotFoundException("TechServe.Database.sql was not found.", schemaPath);
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var exists = connection.CreateCommand();
        exists.CommandText = "SELECT COUNT(*) FROM sys.tables WHERE name = 'Customers'";
        if (Convert.ToInt32(await exists.ExecuteScalarAsync(cancellationToken)) > 0)
        {
            return;
        }

        var schema = await File.ReadAllTextAsync(schemaPath, cancellationToken);
        foreach (var statement in schema.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            await using var command = connection.CreateCommand();
            command.CommandText = statement;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<CustomerRecord>> GetCustomersAsync(CancellationToken cancellationToken)
    {
        var customers = new List<CustomerRecord>();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT CustomerId, FullName, ContactNumber, Email, Address, CustomerStatus,
                   (SELECT COUNT(*) FROM RepairJobs WHERE CustomerId = Customers.CustomerId) AS RepairJobs
            FROM Customers ORDER BY CustomerId DESC
            """;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            customers.Add(new CustomerRecord(
                reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? "" : reader.GetString(2),
                reader.IsDBNull(3) ? "" : reader.GetString(3), reader.IsDBNull(4) ? "" : reader.GetString(4),
                reader.GetString(5), reader.GetInt32(6)));
        }
        return customers;
    }

    public async Task<CustomerRecord> AddCustomerAsync(CustomerInput input, CancellationToken cancellationToken)
    {
        var trimmedName = input.Name.Trim();
        var trimmedContact = input.Contact.Trim();
        var trimmedEmail = input.Email.Trim();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using (var check = connection.CreateCommand())
        {
            check.CommandText = """
                SELECT TOP 1 CustomerId
                FROM Customers
                WHERE LOWER(TRIM(FullName)) = LOWER(@name)
                   OR LOWER(TRIM(ContactNumber)) = LOWER(@contact)
                   OR LOWER(TRIM(Email)) = LOWER(@email)
                """;
            check.Parameters.AddWithValue("@name", trimmedName);
            check.Parameters.AddWithValue("@contact", trimmedContact);
            check.Parameters.AddWithValue("@email", trimmedEmail);
            var existingId = await check.ExecuteScalarAsync(cancellationToken);
            if (existingId is not null)
            {
                throw new InvalidOperationException("A customer with the same contact number or email already exists.");
            }
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Customers (FullName, ContactNumber, Email, Address)
            OUTPUT INSERTED.CustomerId, INSERTED.FullName, INSERTED.ContactNumber,
                   INSERTED.Email, INSERTED.Address, INSERTED.CustomerStatus
            VALUES (@name, @contact, @email, @address);
            """;
        command.Parameters.AddWithValue("@name", trimmedName);
        command.Parameters.AddWithValue("@contact", trimmedContact);
        command.Parameters.AddWithValue("@email", trimmedEmail);
        command.Parameters.AddWithValue("@address", (object?)input.Address ?? DBNull.Value);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return new CustomerRecord(reader.GetInt32(0), reader.GetString(1),
            reader.IsDBNull(2) ? "" : reader.GetString(2), reader.IsDBNull(3) ? "" : reader.GetString(3),
            reader.IsDBNull(4) ? "" : reader.GetString(4), reader.GetString(5), 0);
    }

    public async Task<bool> ArchiveCustomerAsync(int customerId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Customers SET CustomerStatus = 'Archived' WHERE CustomerId = @customerId AND CustomerStatus <> 'Archived';";
        command.Parameters.AddWithValue("@customerId", customerId);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<UserRecord?> FindUserAsync(string username, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT TOP 1 UserId, FullName, Username, Email, PasswordHash, Role, IsActive, InvitationTokenHash, InvitationExpiresAt FROM Users WHERE Username = @identifier OR Email = @identifier";
        command.Parameters.AddWithValue("@identifier", username.Trim().ToLowerInvariant());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new UserRecord(reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
            reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetBoolean(6),
            reader.IsDBNull(7) ? null : reader.GetString(7), reader.IsDBNull(8) ? null : reader.GetDateTime(8));
    }

    public async Task<IReadOnlyList<UserRecord>> GetUsersAsync(CancellationToken cancellationToken)
    {
        var users = new List<UserRecord>();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT UserId, FullName, Username, Email, PasswordHash, Role, IsActive, InvitationTokenHash, InvitationExpiresAt FROM Users ORDER BY FullName";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(new UserRecord(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
                reader.GetString(4), reader.GetString(5), reader.GetBoolean(6), reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.IsDBNull(8) ? null : reader.GetDateTime(8)));
        }
        return users;
    }

    public async Task AddInvitedUserAsync(string username, string email, string fullName, string passwordHash, string role, string tokenHash, DateTime expiresAt, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Users (FullName, Username, Email, PasswordHash, Role, IsActive, InvitationTokenHash, InvitationExpiresAt) VALUES (@fullName, @username, @email, @passwordHash, @role, 1, @tokenHash, @expiresAt);";
        command.Parameters.AddWithValue("@fullName", fullName);
        command.Parameters.AddWithValue("@username", username.Trim().ToLowerInvariant());
        command.Parameters.AddWithValue("@email", email.Trim().ToLowerInvariant());
        command.Parameters.AddWithValue("@passwordHash", passwordHash);
        command.Parameters.AddWithValue("@role", role);
        command.Parameters.AddWithValue("@tokenHash", tokenHash);
        command.Parameters.AddWithValue("@expiresAt", expiresAt);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> DeactivateUserAsync(int userId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Users SET IsActive = 0 WHERE UserId = @userId AND Role <> 'ADMIN';";
        command.Parameters.AddWithValue("@userId", userId);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> DeleteUserAsync(int userId, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        await using (var clearReferences = connection.CreateCommand())
        {
            clearReferences.Transaction = (SqlTransaction)transaction;
            clearReferences.CommandText = "UPDATE CustomerNotes SET CreatedBy = NULL WHERE CreatedBy = @userId; UPDATE RepairStatusHistories SET ChangedBy = NULL WHERE ChangedBy = @userId;";
            clearReferences.Parameters.AddWithValue("@userId", userId);
            await clearReferences.ExecuteNonQueryAsync(cancellationToken);
        }

        await using var delete = connection.CreateCommand();
        delete.Transaction = (SqlTransaction)transaction;
        delete.CommandText = "DELETE FROM Users WHERE UserId = @userId AND Role <> 'ADMIN';";
        delete.Parameters.AddWithValue("@userId", userId);
        var deleted = await delete.ExecuteNonQueryAsync(cancellationToken) == 1;
        await transaction.CommitAsync(cancellationToken);
        return deleted;
    }

    public async Task<bool> CompleteInvitationAsync(string tokenHash, string passwordHash, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Users SET PasswordHash = @passwordHash, InvitationTokenHash = NULL, InvitationExpiresAt = NULL WHERE InvitationTokenHash = @tokenHash AND InvitationExpiresAt > SYSUTCDATETIME() AND IsActive = 1;";
        command.Parameters.AddWithValue("@passwordHash", passwordHash);
        command.Parameters.AddWithValue("@tokenHash", tokenHash);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task UpdatePasswordAsync(string username, string passwordHash, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Users SET PasswordHash = @passwordHash WHERE Username = @identifier OR Email = @identifier;";
        command.Parameters.AddWithValue("@passwordHash", passwordHash);
        command.Parameters.AddWithValue("@identifier", username.Trim().ToLowerInvariant());
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> UpdateUserFullNameAsync(string username, string fullName, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Users SET FullName = @fullName WHERE Username = @identifier OR Email = @identifier;";
        command.Parameters.AddWithValue("@fullName", fullName.Trim());
        command.Parameters.AddWithValue("@identifier", username.Trim().ToLowerInvariant());
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task UpsertSeedUsersAsync(IReadOnlyList<SeedUserRecord> users, CancellationToken cancellationToken)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        foreach (var user in users)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = """
                IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = @email OR Username = @username)
                    INSERT INTO Users (FullName, Username, Email, PasswordHash, Role) VALUES (@fullName, @username, @email, @passwordHash, @role);
                """;
            command.Parameters.AddWithValue("@fullName", user.FullName);
            command.Parameters.AddWithValue("@username", user.Username.ToLowerInvariant());
            command.Parameters.AddWithValue("@email", user.Username.ToLowerInvariant());
            command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@role", user.Role);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}

public sealed record CustomerInput(string Name, string Contact, string Email, string? Address);
public sealed record CustomerRecord(int Id, string Name, string Contact, string Email, string Address, string Status, int Jobs);
public sealed record UserRecord(int Id, string FullName, string Username, string Email, string PasswordHash, string Role, bool IsActive = true, string? InvitationTokenHash = null, DateTime? InvitationExpiresAt = null);
public sealed record SeedUserRecord(string FullName, string Username, string PasswordHash, string Role);
