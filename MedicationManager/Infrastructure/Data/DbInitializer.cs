using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedicationManager.Infrastructure.Data
{
    /// <summary>
    /// Handles database initialization and seeding
    /// </summary>
    public class DbInitializer
    {
        private readonly MedicationDbContext _context;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(MedicationDbContext context, ILogger<DbInitializer> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Initialize the database and create indexes
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Starting database initialization...");

                // Create database if it doesn't exist
                await _context.Database.EnsureCreatedAsync();

                _logger.LogInformation("Database created/verified successfully");

                // Apply any pending migrations
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("Applying {Count} pending migrations...", pendingMigrations.Count());
                    await _context.Database.MigrateAsync();
                    _logger.LogInformation("Migrations applied successfully");
                }

                _logger.LogInformation("Database initialization completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database");
                throw;
            }
        }

        /// <summary>
        /// Verify that all indexes are created
        /// </summary>
        public async Task<bool> VerifyIndexesAsync()
        {
            try
            {
                _logger.LogInformation("Verifying database indexes...");

                // Execute a query to verify indexes exist (SQLite specific)
                var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT name, tbl_name 
                    FROM sqlite_master 
                    WHERE type = 'index' 
                    AND name LIKE 'IX_%'
                    ORDER BY tbl_name, name";

                using var reader = await command.ExecuteReaderAsync();

                int indexCount = 0;
                while (await reader.ReadAsync())
                {
                    var indexName = reader.GetString(0);
                    var tableName = reader.GetString(1);
                    _logger.LogDebug("Found index: {IndexName} on table {TableName}", indexName, tableName);
                    indexCount++;
                }

                _logger.LogInformation("Verified {Count} indexes in database", indexCount);
                return indexCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying indexes");
                return false;
            }
        }
    }
}