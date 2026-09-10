using Microsoft.Data.SqlClient;

public sealed class TechServeDatabase
{
    private readonly string connectionString;
    private readonly string schemaPath;

    public TechServeDatabase(IConfiguration configuration, IWebHostEnvironment environment)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");
        schemaPath = Path.Combine(environment.ContentRootPath, "TechServe.Database.sql");
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;
        builder.InitialCatalog = "master";

        await using (var master = new SqlConnection(builder.ConnectionString))
        {
            await master.OpenAsync(cancellationToken);
            await using var create = master.CreateCommand();
            create.CommandText = $"IF DB_ID(@name) IS NULL CREATE DATABASE [{databaseName.Replace("]", "]]")}]";
            create.Parameters.AddWithValue("@name", databaseName);
            await create.ExecuteNonQueryAsync(cancellationToken);
        }

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
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Customers (FullName, ContactNumber, Email, Address)
            OUTPUT INSERTED.CustomerId, INSERTED.FullName, INSERTED.ContactNumber,
                   INSERTED.Email, INSERTED.Address, INSERTED.CustomerStatus
            VALUES (@name, @contact, @email, @address);
            """;
        command.Parameters.AddWithValue("@name", input.Name);
        command.Parameters.AddWithValue("@contact", input.Contact);
        command.Parameters.AddWithValue("@email", input.Email);
        command.Parameters.AddWithValue("@address", (object?)input.Address ?? DBNull.Value);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return new CustomerRecord(reader.GetInt32(0), reader.GetString(1),
            reader.IsDBNull(2) ? "" : reader.GetString(2), reader.IsDBNull(3) ? "" : reader.GetString(3),
            reader.IsDBNull(4) ? "" : reader.GetString(4), reader.GetString(5), 0);
    }
}

public sealed record CustomerInput(string Name, string Contact, string Email, string? Address);
public sealed record CustomerRecord(int Id, string Name, string Contact, string Email, string Address, string Status, int Jobs);
