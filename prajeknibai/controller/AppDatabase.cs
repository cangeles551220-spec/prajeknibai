using System.Configuration;
using Microsoft.Data.SqlClient;

namespace prajeknibai.controller
{
    internal static class AppDatabase
    {
        private const string ConnectionStringName = "AppDb";

        private const string DefaultDatabaseName = "prajeknibai";

        internal static string DatabaseConnectionString =>
            TryGetConfiguredConnectionString() ?? BuildLocalDbFallbackConnectionString();

        private static string? TryGetConfiguredConnectionString()
        {
            var cs = ConfigurationManager.ConnectionStrings[ConnectionStringName]?.ConnectionString;
            return string.IsNullOrWhiteSpace(cs) ? null : cs;
        }

        private static string BuildLocalDbFallbackConnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = @"(localdb)\MSSQLLocalDB",
                InitialCatalog = DefaultDatabaseName,
                IntegratedSecurity = true,
                TrustServerCertificate = true,
                ConnectTimeout = 5
            };

            return builder.ConnectionString;
        }

        internal static SqlConnection GetConnection() => new SqlConnection(DatabaseConnectionString);

        internal static void EnsureDatabaseExists()
        {
            var appBuilder = new SqlConnectionStringBuilder(DatabaseConnectionString);
            var databaseName = string.IsNullOrWhiteSpace(appBuilder.InitialCatalog)
                ? DefaultDatabaseName
                : appBuilder.InitialCatalog;

            // Connect to master to create the app DB if it doesn't exist.
            var masterBuilder = new SqlConnectionStringBuilder(DatabaseConnectionString)
            {
                InitialCatalog = "master"
            };

            using (var connection = new SqlConnection(masterBuilder.ConnectionString))
            {
                connection.Open();

                var safeDatabaseIdentifier = "[" + databaseName.Replace("]", "]]", StringComparison.Ordinal) + "]";
                using var command = new SqlCommand(
                    $"IF DB_ID(@dbName) IS NULL BEGIN CREATE DATABASE {safeDatabaseIdentifier} END",
                    connection);
                command.Parameters.AddWithValue("@dbName", databaseName);
                command.ExecuteNonQuery();
            }

            // Validate we can open the app DB.
            using var appConnection = new SqlConnection(DatabaseConnectionString);
            appConnection.Open();
        }

        internal static void BackupDatabase(string backupPath)
        {
            var builder = new SqlConnectionStringBuilder(DatabaseConnectionString);
            var databaseName = builder.InitialCatalog;

            using var connection = new SqlConnection(DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(
                $"BACKUP DATABASE [{databaseName}] TO DISK = @Path WITH INIT;",
                connection);

            command.Parameters.AddWithValue("@Path", backupPath);
            command.ExecuteNonQuery();
        }
    }
}