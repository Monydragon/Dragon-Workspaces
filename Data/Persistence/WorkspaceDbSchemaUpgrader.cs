#if BROWSER
using System.Threading;
using System.Threading.Tasks;

namespace Dragon_Workspaces.Data.Persistence;

internal static class WorkspaceDbSchemaUpgrader
{
    public static Task EnsureCompatibleSchemaAsync(
        WorkspaceDbContext dbContext,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
#else
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Dragon_Workspaces.Data.Persistence;

internal static class WorkspaceDbSchemaUpgrader
{
    public static async Task EnsureCompatibleSchemaAsync(
        WorkspaceDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (!dbContext.Database.IsSqlite())
        {
            return;
        }

        var connection = (SqliteConnection)dbContext.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != ConnectionState.Open;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            var projectColumns = await GetTableColumnsAsync(connection, "Projects", cancellationToken);

            if (projectColumns.Count == 0)
            {
                return;
            }

            await EnsureProjectColumnAsync(
                connection,
                projectColumns,
                "FolderBookmarkId",
                "TEXT NULL",
                cancellationToken);

            await EnsureProjectColumnAsync(
                connection,
                projectColumns,
                "Metadata",
                "TEXT NOT NULL DEFAULT '{}'",
                cancellationToken);

            if (!projectColumns.Contains("LastSynced"))
            {
                await ExecuteNonQueryAsync(
                    connection,
                    "ALTER TABLE \"Projects\" ADD COLUMN \"LastSynced\" TEXT NOT NULL DEFAULT '1970-01-01T00:00:00.0000000Z';",
                    cancellationToken);

                await ExecuteNonQueryAsync(
                    connection,
                    "UPDATE \"Projects\" SET \"LastSynced\" = \"LastAccessed\";",
                    cancellationToken);

                projectColumns.Add("LastSynced");
            }

            await EnsureProjectColumnAsync(
                connection,
                projectColumns,
                "WorkstationType",
                "TEXT NOT NULL DEFAULT 'Other'",
                cancellationToken);

            await EnsureProjectColumnAsync(
                connection,
                projectColumns,
                "TypeIcon",
                "TEXT NOT NULL DEFAULT 'GEN'",
                cancellationToken);

            await ExecuteNonQueryAsync(
                connection,
                "CREATE INDEX IF NOT EXISTS \"IX_Projects_Path\" ON \"Projects\" (\"Path\");",
                cancellationToken);

            await ExecuteNonQueryAsync(
                connection,
                "CREATE UNIQUE INDEX IF NOT EXISTS \"IX_Projects_FolderBookmarkId\" ON \"Projects\" (\"FolderBookmarkId\");",
                cancellationToken);
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static async Task<HashSet<string>> GetTableColumnsAsync(
        SqliteConnection connection,
        string tableName,
        CancellationToken cancellationToken)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info(\"{tableName.Replace("\"", "\"\"")}\");";

        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }

    private static async Task EnsureProjectColumnAsync(
        SqliteConnection connection,
        ISet<string> projectColumns,
        string columnName,
        string columnDefinition,
        CancellationToken cancellationToken)
    {
        if (projectColumns.Contains(columnName))
        {
            return;
        }

        await ExecuteNonQueryAsync(
            connection,
            $"ALTER TABLE \"Projects\" ADD COLUMN \"{columnName.Replace("\"", "\"\"")}\" {columnDefinition};",
            cancellationToken);

        projectColumns.Add(columnName);
    }

    private static async Task ExecuteNonQueryAsync(
        SqliteConnection connection,
        string commandText,
        CancellationToken cancellationToken)
    {
        using var command = connection.CreateCommand();
        command.CommandText = commandText;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
#endif
