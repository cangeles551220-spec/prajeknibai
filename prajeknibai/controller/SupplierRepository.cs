using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace prajeknibai.controller
{
    internal sealed class AppSupplier
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string ContactNumber { get; init; } = string.Empty;
    }

    internal static class SupplierRepository
    {
        public static IReadOnlyList<AppSupplier> GetSuppliers()
        {
            EnsureDatabase();

            var suppliers = new List<AppSupplier>();
            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand("SELECT Id, Name, ContactNumber FROM dbo.Suppliers ORDER BY Name;", connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                suppliers.Add(new AppSupplier
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    ContactNumber = reader.GetString(2)
                });
            }

            return suppliers;
        }

        public static bool AddSupplier(string name, string contactNumber)
        {
            EnsureDatabase();

            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(@"
INSERT INTO dbo.Suppliers (Name, ContactNumber)
VALUES (@Name, @ContactNumber);", connection);

            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@ContactNumber", contactNumber);

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
            AppDatabase.EnsureDatabaseExists();
            EnsureSuppliersTableExists();
            SeedDefaultSuppliers();
        }

        private static void EnsureSuppliersTableExists()
        {
            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(@"
IF OBJECT_ID(N'dbo.Suppliers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Suppliers
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL UNIQUE,
        ContactNumber NVARCHAR(50) NOT NULL
    );
END;", connection);

            command.ExecuteNonQuery();
        }

        private static void SeedDefaultSuppliers()
        {
            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Suppliers)
BEGIN
    INSERT INTO dbo.Suppliers (Name, ContactNumber)
    VALUES
        (N'Supplier A', N'555-1234'),
        (N'Supplier B', N'555-5678');
END;", connection);

            command.ExecuteNonQuery();
        }
    }
}
