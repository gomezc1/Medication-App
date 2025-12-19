using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MedicationManager.Infrastructure.Data
{
    /// <summary>
    /// Factory for creating DbContext at design time (for migrations)
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MedicationDbContext>
    {
        public MedicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MedicationDbContext>();

            // Use SQLite connection string
            optionsBuilder.UseSqlite("Data Source=medications.db");

            return new MedicationDbContext(optionsBuilder.Options);
        }
    }
}