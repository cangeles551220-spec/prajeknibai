using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace prajeknibai.controller
{
    internal sealed class BackupHistoryEntry
    {
        public DateTime CreatedAt { get; init; }
        public string FileName { get; init; } = string.Empty;
        public string FullPath { get; init; } = string.Empty;
    }

    internal static class BackupHistoryRepository
    {
        public static IReadOnlyList<BackupHistoryEntry> GetBackupHistory()
        {
            EnsureDatabase();

            var history = new List<BackupHistoryEntry>();

            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(@"
SELECT CreatedAt, FileName, FullPath
FROM dbo.BackupHistory
ORDER BY CreatedAt DESC;", connection);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                history.Add(new BackupHistoryEntry
                {
                    CreatedAt = reader.GetDateTime(0),
                    FileName = reader.GetString(1),
                    FullPath = reader.GetString(2)
                });
            }

            return history;
        }

        public static void AddBackupHistory(string fileName, string fullPath, DateTime createdAt)
        {
            EnsureDatabase();

            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(@"
INSERT INTO dbo.BackupHistory (CreatedAt, FileName, FullPath)
VALUES (@CreatedAt, @FileName, @FullPath);", connection);

            command.Parameters.AddWithValue("@CreatedAt", createdAt);
            command.Parameters.AddWithValue("@FileName", fileName);
            command.Parameters.AddWithValue("@FullPath", fullPath);
            command.ExecuteNonQuery();
        }

        private static void EnsureDatabase()
        {
            AppDatabase.EnsureDatabaseExists();
            EnsureBackupHistoryTableExists();
        }

        private static void EnsureBackupHistoryTableExists()
        {
            using var connection = new SqlConnection(AppDatabase.DatabaseConnectionString);
            connection.Open();

            using var command = new SqlCommand(@"
IF OBJECT_ID(N'dbo.BackupHistory', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BackupHistory
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CreatedAt DATETIME2 NOT NULL,
        FileName NVARCHAR(260) NOT NULL,
        FullPath NVARCHAR(500) NOT NULL
    );
END;", connection);

            command.ExecuteNonQuery();
        }
    }
}
