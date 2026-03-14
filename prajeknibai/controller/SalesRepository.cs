using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace prajeknibai.controller
{
    internal sealed class SalesSummary
    {
        public decimal TotalRevenue { get; init; }
        public int TotalSales { get; init; }
        public decimal AverageSale { get; init; }
    }

    internal sealed class RecentSale
    {
        public int Id { get; init; }
        public DateTime CreatedAt { get; init; }
        public decimal Total { get; init; }
        public string PaymentMethod { get; init; } = string.Empty;
        public int ItemCount { get; init; }
    }

    internal sealed class SaleLine
    {
        public string ProductName { get; init; } = string.Empty;
        public string Sku { get; init; } = string.Empty;
        public decimal UnitPrice { get; init; }
        public int Quantity { get; init; }
    }

    internal static class SalesRepository
    {
        public static SalesSummary GetSalesSummary()
        {
            EnsureDatabase();

            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(@"
SELECT
    ISNULL(SUM(Total), 0),
    COUNT(*),
    ISNULL(AVG(CAST(Total AS DECIMAL(18,2))), 0)
FROM dbo.Sales;", connection);

            using var reader = command.ExecuteReader();
            reader.Read();

            return new SalesSummary
            {
                TotalRevenue = reader.GetDecimal(0),
                TotalSales = reader.GetInt32(1),
                AverageSale = reader.GetDecimal(2)
            };
        }

        public static IReadOnlyList<RecentSale> GetRecentSales(int count)
        {
            EnsureDatabase();

            var sales = new List<RecentSale>();

            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(@"
SELECT TOP (@Count)
    s.Id,
    s.CreatedAt,
    s.Total,
    s.PaymentMethod,
    ISNULL(SUM(si.Quantity), 0) AS ItemCount
FROM dbo.Sales s
LEFT JOIN dbo.SaleItems si ON si.SaleId = s.Id
GROUP BY s.Id, s.CreatedAt, s.Total, s.PaymentMethod
ORDER BY s.CreatedAt DESC;", connection);

            command.Parameters.AddWithValue("@Count", count);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                sales.Add(new RecentSale
                {
                    Id = reader.GetInt32(0),
                    CreatedAt = reader.GetDateTime(1),
                    Total = reader.GetDecimal(2),
                    PaymentMethod = reader.GetString(3),
                    ItemCount = reader.GetInt32(4)
                });
            }

            return sales;
        }

        public static int GetTodaySalesCount()
        {
            EnsureDatabase();

            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(@"
SELECT COUNT(*)
FROM dbo.Sales
WHERE CONVERT(date, CreatedAt) = CONVERT(date, SYSUTCDATETIME());", connection);

            return (int)command.ExecuteScalar();
        }

        public static void SaveSale(IReadOnlyCollection<SaleLine> saleLines, decimal subtotal, decimal tax, decimal total, string paymentMethod)
        {
            EnsureDatabase();

            if (saleLines.Count == 0)
            {
                return;
            }

            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            var productIdsBySku = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in saleLines)
            {
                using var stockCommand = new SqlCommand(@"
SELECT Id, Stock
FROM dbo.Products
WHERE Sku = @Sku;", connection, transaction);

                stockCommand.Parameters.AddWithValue("@Sku", line.Sku);
                using var reader = stockCommand.ExecuteReader();
                if (!reader.Read())
                {
                    throw new InvalidOperationException($"Product '{line.ProductName}' no longer exists.");
                }

                var productId = reader.GetInt32(0);
                var availableStock = reader.GetInt32(1);
                if (availableStock < line.Quantity)
                {
                    throw new InvalidOperationException($"Not enough stock for '{line.ProductName}'. Available: {availableStock}.");
                }

                productIdsBySku[line.Sku] = productId;
            }

            using var saleCommand = new SqlCommand(@"
INSERT INTO dbo.Sales (Subtotal, Tax, Total, PaymentMethod)
OUTPUT INSERTED.Id
VALUES (@Subtotal, @Tax, @Total, @PaymentMethod);", connection, transaction);

            saleCommand.Parameters.AddWithValue("@Subtotal", subtotal);
            saleCommand.Parameters.AddWithValue("@Tax", tax);
            saleCommand.Parameters.AddWithValue("@Total", total);
            saleCommand.Parameters.AddWithValue("@PaymentMethod", paymentMethod);

            var saleId = (int)saleCommand.ExecuteScalar();

            foreach (var line in saleLines)
            {
                using var lineCommand = new SqlCommand(@"
INSERT INTO dbo.SaleItems (SaleId, ProductId, ProductName, Sku, UnitPrice, Quantity)
VALUES (@SaleId, @ProductId, @ProductName, @Sku, @UnitPrice, @Quantity);", connection, transaction);

                lineCommand.Parameters.AddWithValue("@SaleId", saleId);
                lineCommand.Parameters.AddWithValue("@ProductId", productIdsBySku[line.Sku]);
                lineCommand.Parameters.AddWithValue("@ProductName", line.ProductName);
                lineCommand.Parameters.AddWithValue("@Sku", line.Sku);
                lineCommand.Parameters.AddWithValue("@UnitPrice", line.UnitPrice);
                lineCommand.Parameters.AddWithValue("@Quantity", line.Quantity);
                lineCommand.ExecuteNonQuery();

                ProductRepository.ReduceStock(line.Sku, line.Quantity, connection, transaction);
            }

            transaction.Commit();
        }

        private static void EnsureDatabase()
        {
            AppDatabase.EnsureDatabaseExists();
            ProductRepository.GetProducts();
            EnsureSalesTablesExist();
        }

        private static void EnsureSalesTablesExist()
        {
            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var createTablesCommand = new SqlCommand(@"
IF OBJECT_ID(N'dbo.Sales', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Sales
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        Subtotal DECIMAL(18,2) NOT NULL,
        Tax DECIMAL(18,2) NOT NULL,
        Total DECIMAL(18,2) NOT NULL,
        PaymentMethod NVARCHAR(50) NOT NULL
    );
END;

IF OBJECT_ID(N'dbo.SaleItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.SaleItems
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SaleId INT NOT NULL,
        ProductId INT NOT NULL,
        ProductName NVARCHAR(100) NOT NULL,
        Sku NVARCHAR(50) NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        Quantity INT NOT NULL,
        CONSTRAINT FK_SaleItems_Sales FOREIGN KEY (SaleId) REFERENCES dbo.Sales(Id),
        CONSTRAINT FK_SaleItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
    );
END;", connection);

            createTablesCommand.ExecuteNonQuery();

            using var addProductIdColumnCommand = new SqlCommand(@"
IF COL_LENGTH(N'dbo.SaleItems', N'ProductId') IS NULL
BEGIN
    ALTER TABLE dbo.SaleItems
    ADD ProductId INT NULL;
END;", connection);
            addProductIdColumnCommand.ExecuteNonQuery();

            using var populateProductIdCommand = new SqlCommand(@"
IF COL_LENGTH(N'dbo.SaleItems', N'ProductId') IS NOT NULL
BEGIN
    UPDATE si
    SET ProductId = p.Id
    FROM dbo.SaleItems si
    INNER JOIN dbo.Products p ON p.Sku = si.Sku
    WHERE si.ProductId IS NULL;
END;", connection);
            populateProductIdCommand.ExecuteNonQuery();

            using var addProductForeignKeyCommand = new SqlCommand(@"
IF COL_LENGTH(N'dbo.SaleItems', N'ProductId') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_SaleItems_Products')
BEGIN
    ALTER TABLE dbo.SaleItems
    WITH NOCHECK ADD CONSTRAINT FK_SaleItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id);
END;", connection);
            addProductForeignKeyCommand.ExecuteNonQuery();
        }
    }
}
