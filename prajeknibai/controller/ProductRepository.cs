using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace prajeknibai.controller
{
    internal sealed class AppProduct
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Sku { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public int Stock { get; init; }
        public int SupplierId { get; init; }
        public string SupplierName { get; init; } = string.Empty;
    }

    internal static class ProductRepository
    {
        public static IReadOnlyList<AppProduct> GetProducts()
        {
            EnsureDatabase();

            var products = new List<AppProduct>();
            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(@"
SELECT p.Id, p.Name, p.Sku, p.Price, p.Stock, p.SupplierId, s.Name
FROM dbo.Products p
LEFT JOIN dbo.Suppliers s ON s.Id = p.SupplierId
ORDER BY p.Name;", connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                products.Add(new AppProduct
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Sku = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    Stock = reader.GetInt32(4),
                    SupplierId = reader.GetInt32(5),
                    SupplierName = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                });
            }

            return products;
        }

        public static bool AddProduct(string name, string sku, decimal price, int stock, int supplierId)
        {
            EnsureDatabase();

            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(@"
INSERT INTO dbo.Products (Name, Sku, Price, Stock, SupplierId)
VALUES (@Name, @Sku, @Price, @Stock, @SupplierId);", connection);

            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@Sku", sku);
            command.Parameters.AddWithValue("@Price", price);
            command.Parameters.AddWithValue("@Stock", stock);
            command.Parameters.AddWithValue("@SupplierId", supplierId);

            try
            {
                return command.ExecuteNonQuery() == 1;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                return false;
            }
        }

        public static bool UpdateProduct(int id, string name, string sku, decimal price, int stock, int supplierId)
        {
            EnsureDatabase();

            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(@"
UPDATE dbo.Products
SET Name = @Name,
    Sku = @Sku,
    Price = @Price,
    Stock = @Stock,
    SupplierId = @SupplierId
WHERE Id = @Id;", connection);

            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@Sku", sku);
            command.Parameters.AddWithValue("@Price", price);
            command.Parameters.AddWithValue("@Stock", stock);
            command.Parameters.AddWithValue("@SupplierId", supplierId);

            try
            {
                return command.ExecuteNonQuery() == 1;
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                return false;
            }
        }

        public static void DeleteProduct(int id)
        {
            EnsureDatabase();

            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand("DELETE FROM dbo.Products WHERE Id = @Id;", connection);
            command.Parameters.AddWithValue("@Id", id);
            command.ExecuteNonQuery();
        }

        public static void ReduceStock(string sku, int quantity, SqlConnection connection, SqlTransaction transaction)
        {
            using var command = new SqlCommand(@"
UPDATE dbo.Products
SET Stock = CASE WHEN Stock >= @Quantity THEN Stock - @Quantity ELSE 0 END
WHERE Sku = @Sku;", connection, transaction);

            command.Parameters.AddWithValue("@Sku", sku);
            command.Parameters.AddWithValue("@Quantity", quantity);
            command.ExecuteNonQuery();
        }

        private static void EnsureDatabase()
        {
            AppDatabase.EnsureDatabaseExists();
            SupplierRepository.GetSuppliers();
            EnsureProductsTableExists();
            SeedDefaultProducts();
        }

        private static void EnsureProductsTableExists()
        {
            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var createTableCommand = new SqlCommand(@"
IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Sku NVARCHAR(50) NOT NULL UNIQUE,
        Price DECIMAL(18,2) NOT NULL,
        Stock INT NOT NULL DEFAULT 0,
        SupplierId INT NULL
    );
END;", connection);

            createTableCommand.ExecuteNonQuery();

            using var addSupplierColumnCommand = new SqlCommand(@"
IF COL_LENGTH(N'dbo.Products', N'SupplierId') IS NULL
BEGIN
    ALTER TABLE dbo.Products
    ADD SupplierId INT NULL;
END;", connection);
            addSupplierColumnCommand.ExecuteNonQuery();

            using var addPriceConstraintCommand = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Products_Price_NonNegative')
BEGIN
    ALTER TABLE dbo.Products
    ADD CONSTRAINT CK_Products_Price_NonNegative CHECK (Price >= 0);
END;", connection);
            addPriceConstraintCommand.ExecuteNonQuery();

            using var addStockConstraintCommand = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Products_Stock_NonNegative')
BEGIN
    ALTER TABLE dbo.Products
    ADD CONSTRAINT CK_Products_Stock_NonNegative CHECK (Stock >= 0);
END;", connection);
            addStockConstraintCommand.ExecuteNonQuery();

            using var populateSupplierCommand = new SqlCommand(@"
IF COL_LENGTH(N'dbo.Products', N'SupplierId') IS NOT NULL
   AND EXISTS (SELECT 1 FROM dbo.Suppliers)
BEGIN
    DECLARE @DefaultSupplierId INT = (SELECT TOP 1 Id FROM dbo.Suppliers ORDER BY Id);
    UPDATE dbo.Products
    SET SupplierId = @DefaultSupplierId
    WHERE SupplierId IS NULL;
END;", connection);
            populateSupplierCommand.ExecuteNonQuery();

            using var addForeignKeyCommand = new SqlCommand(@"
IF COL_LENGTH(N'dbo.Products', N'SupplierId') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Products_Suppliers')
BEGIN
    ALTER TABLE dbo.Products
    WITH NOCHECK ADD CONSTRAINT FK_Products_Suppliers FOREIGN KEY (SupplierId) REFERENCES dbo.Suppliers(Id);
END;", connection);
            addForeignKeyCommand.ExecuteNonQuery();

            using var enforceSupplierNotNullCommand = new SqlCommand(@"
IF COL_LENGTH(N'dbo.Products', N'SupplierId') IS NOT NULL
   AND EXISTS (SELECT 1 FROM dbo.Suppliers)
   AND NOT EXISTS (SELECT 1 FROM dbo.Products WHERE SupplierId IS NULL)
BEGIN
    ALTER TABLE dbo.Products
    ALTER COLUMN SupplierId INT NOT NULL;
END;", connection);
            enforceSupplierNotNullCommand.ExecuteNonQuery();
        }

        private static void SeedDefaultProducts()
        {
            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Products)
BEGIN
    DECLARE @SupplierAId INT = ISNULL((SELECT TOP 1 Id FROM dbo.Suppliers WHERE Name = N'Supplier A' ORDER BY Id), (SELECT TOP 1 Id FROM dbo.Suppliers ORDER BY Id));
    DECLARE @SupplierBId INT = ISNULL((SELECT TOP 1 Id FROM dbo.Suppliers WHERE Name = N'Supplier B' ORDER BY Id), @SupplierAId);

    INSERT INTO dbo.Products (Name, Sku, Price, Stock, SupplierId)
    VALUES
        (N'Product A', N'SKU-1001', 50.00, 45, @SupplierAId),
        (N'Product B', N'SKU-1002', 35.00, 30, @SupplierBId),
        (N'Product C', N'SKU-1003', 75.00, 20, @SupplierAId),
        (N'Product D', N'SKU-1004', 25.00, 45, @SupplierBId),
        (N'Product E', N'SKU-1005', 100.00, 45, @SupplierAId),
        (N'Product F', N'SKU-1006', 40.00, 35, @SupplierBId),
        (N'Product G', N'SKU-1007', 40.00, 35, @SupplierAId),
        (N'Product H', N'SKU-1008', 40.00, 35, @SupplierBId),
        (N'Product I', N'SKU-1009', 40.00, 35, @SupplierAId),
        (N'Product J', N'SKU-1010', 40.00, 35, @SupplierBId),
        (N'Product K', N'SKU-1011', 40.00, 35, @SupplierAId),
        (N'Product L', N'SKU-1012', 40.00, 35, @SupplierBId);
END;", connection);

            command.ExecuteNonQuery();
        }
    }
}
