using MedicationManager.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicationManager.Infrastructure.Data
{
    /// <summary>
    /// Database context for the Medication Manager application
    /// </summary>
    public class MedicationDbContext : DbContext
    {
        public MedicationDbContext(DbContextOptions<MedicationDbContext> options)
            : base(options)
        {
        }

        // DbSet properties
        public DbSet<Medication> Medications { get; set; } = null!;
        public DbSet<UserMedication> UserMedications { get; set; } = null!;
        public DbSet<DrugInteraction> DrugInteractions { get; set; } = null!;
        public DbSet<MedicationSchedule> MedicationSchedules { get; set; } = null!;
        public DbSet<MedicationSet> MedicationSets { get; set; } = null!;
        public DbSet<ActiveIngredientDuplicate> ActiveIngredientDuplicates { get; set; } = null!;
        public DbSet<AppSetting> AppSettings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ================================================================
            // MEDICATION ENTITY CONFIGURATION
            // ================================================================

            modelBuilder.Entity<Medication>(entity =>
            {
                // Primary Key
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.RxCui)
                    .IsUnique()
                    .HasDatabaseName("IX_Medications_RxCui");

                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("IX_Medications_Name");

                entity.HasIndex(e => e.GenericName)
                    .HasDatabaseName("IX_Medications_GenericName");

                entity.HasIndex(e => e.IsOTC)
                    .HasDatabaseName("IX_Medications_IsOTC");

                // Relationships
                entity.HasMany(e => e.UserMedications)
                    .WithOne(e => e.Medication)
                    .HasForeignKey(e => e.MedicationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ================================================================
            // USER MEDICATION ENTITY CONFIGURATION
            // ================================================================

            modelBuilder.Entity<UserMedication>(entity =>
            {
                // Primary Key
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.MedicationId)
                    .HasDatabaseName("IX_UserMedications_MedicationId");

                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_UserMedications_IsActive");

                entity.HasIndex(e => new { e.MedicationId, e.IsActive })
                    .HasDatabaseName("IX_UserMedications_MedicationId_IsActive");

                // Relationships
                entity.HasOne(e => e.Medication)
                    .WithMany(e => e.UserMedications)
                    .HasForeignKey(e => e.MedicationId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Schedules)
                    .WithOne(e => e.UserMedication)
                    .HasForeignKey(e => e.UserMedicationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ================================================================
            // DRUG INTERACTION ENTITY CONFIGURATION
            // ================================================================

            modelBuilder.Entity<DrugInteraction>(entity =>
            {
                // Primary Key
                entity.HasKey(e => e.Id);

                // Indexes for individual RxCuis (for fast lookup)
                entity.HasIndex(e => e.Drug1RxCui)
                    .HasDatabaseName("IX_DrugInteractions_Drug1RxCui");

                entity.HasIndex(e => e.Drug2RxCui)
                    .HasDatabaseName("IX_DrugInteractions_Drug2RxCui");

                // Composite unique index to prevent duplicate interaction entries
                entity.HasIndex(e => new { e.Drug1RxCui, e.Drug2RxCui })
                    .IsUnique()
                    .HasDatabaseName("IX_DrugInteractions_Drug1_Drug2_Unique");

                // Index on severity level for filtering
                entity.HasIndex(e => e.SeverityLevel)
                    .HasDatabaseName("IX_DrugInteractions_SeverityLevel");

                entity.HasIndex(e => e.ModifiedDate)
                   .HasDatabaseName("IX_DrugInteractions_ModifiedDate");
            });

            // ================================================================
            // MEDICATION SCHEDULE ENTITY CONFIGURATION
            // ================================================================

            modelBuilder.Entity<MedicationSchedule>(entity =>
            {
                // Primary Key
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.UserMedicationId)
                    .HasDatabaseName("IX_MedicationSchedules_UserMedicationId");

                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_MedicationSchedules_IsActive");

                entity.HasIndex(e => new { e.UserMedicationId, e.IsActive })
                    .HasDatabaseName("IX_MedicationSchedules_UserMedicationId_IsActive");

                entity.HasIndex(e => e.GeneratedDate)
                    .HasDatabaseName("IX_MedicationSchedules_GeneratedDate");

                // Relationships
                entity.HasOne(e => e.UserMedication)
                    .WithMany(e => e.Schedules)
                    .HasForeignKey(e => e.UserMedicationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ================================================================
            // MEDICATION SET ENTITY CONFIGURATION
            // ================================================================

            modelBuilder.Entity<MedicationSet>(entity =>
            {
                // Primary Key
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.Name)
                    .HasDatabaseName("IX_MedicationSets_Name");

                entity.HasIndex(e => e.CreatedDate)
                    .HasDatabaseName("IX_MedicationSets_CreatedDate");

                entity.HasIndex(e => e.ModifiedDate)
                    .HasDatabaseName("IX_MedicationSets_ModifiedDate");
            });

            // ================================================================
            // ACTIVE INGREDIENT DUPLICATE ENTITY CONFIGURATION
            // ================================================================

            modelBuilder.Entity<ActiveIngredientDuplicate>(entity =>
            {
                // Primary Key
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.RxCui1)
                    .HasDatabaseName("IX_ActiveIngredientDuplicates_RxCui1");

                entity.HasIndex(e => e.RxCui2)
                    .HasDatabaseName("IX_ActiveIngredientDuplicates_RxCui2");

                entity.HasIndex(e => e.SharedIngredient)
                    .HasDatabaseName("IX_ActiveIngredientDuplicates_SharedIngredient");

                // Composite unique index to prevent duplicate entries
                entity.HasIndex(e => new { e.RxCui1, e.RxCui2, e.SharedIngredient })
                    .IsUnique()
                    .HasDatabaseName("IX_ActiveIngredientDuplicates_Unique");
            });

            // ================================================================
            // APP SETTING ENTITY CONFIGURATION
            // ================================================================

            modelBuilder.Entity<AppSetting>(entity =>
            {
                // Primary Key
                entity.HasKey(e => e.Id);

                // Unique index on SettingKey
                entity.HasIndex(e => e.SettingKey)
                    .IsUnique()
                    .HasDatabaseName("IX_AppSettings_SettingKey");
            });

            // ================================================================
            // ADDITIONAL CONFIGURATION
            // ================================================================

            // Configure datetime conversion for SQLite
            ConfigureDateTimeConversion(modelBuilder);

            // Configure enum to string conversion
            ConfigureEnumConversion(modelBuilder);
        }

        /// <summary>
        /// Configure DateTime properties for SQLite storage
        /// </summary>
        private void ConfigureDateTimeConversion(ModelBuilder modelBuilder)
        {
            // SQLite stores DateTime as TEXT, this ensures proper formatting
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("TEXT");
                    }
                }
            }
        }

        /// <summary>
        /// Configure Enum properties to be stored as strings in SQLite
        /// </summary>
        private void ConfigureEnumConversion(ModelBuilder modelBuilder)
        {
            // TimingPreference enum
            modelBuilder.Entity<UserMedication>()
                .Property(e => e.TimingPreferencesJson)
                .HasColumnType("TEXT");

            // InteractionSeverity enum
            modelBuilder.Entity<DrugInteraction>()
                .Property(e => e.SeverityLevel)
                .HasConversion<string>()
                .HasColumnType("TEXT");

            // TimingPreference in MedicationSchedule
            modelBuilder.Entity<MedicationSchedule>()
                .Property(e => e.TimeSlot)
                .HasConversion<string>()
                .HasColumnType("TEXT");
        }

        /// <summary>
        /// Override SaveChanges to automatically update ModifiedDate
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically update ModifiedDate
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Automatically update CreatedDate and ModifiedDate timestamps
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                var now = DateTime.Now;

                if (entry.State == EntityState.Added)
                {
                    // Set CreatedDate for new entities
                    if (entry.Property("CreatedDate") != null)
                    {
                        entry.Property("CreatedDate").CurrentValue = now;
                    }
                }

                // Always update ModifiedDate
                if (entry.Property("ModifiedDate") != null)
                {
                    entry.Property("ModifiedDate").CurrentValue = now;
                }
            }
        }
    }
}


// ============================================================================
// File: MedicationManager.Infrastructure/Data/DbInitializer.cs
// Description: Database initialization and seeding
// ============================================================================



/*
 * USAGE INSTRUCTIONS:
 * 
 * 1. Add this DbContext to your dependency injection in Program.cs or App.xaml.cs:
 * 
 *    services.AddDbContext<MedicationDbContext>(options =>
 *        options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
 * 
 * 2. Create initial migration:
 *    In Package Manager Console:
 *    Add-Migration InitialCreate
 *    Update-Database
 * 
 * 3. The indexes will be created automatically when the migration is applied
 * 
 * 4. To verify indexes were created, you can use SQLite browser or run:
 *    SELECT name FROM sqlite_master WHERE type='index' AND name LIKE 'IX_%';
 * 
 * INDEX SUMMARY:
 * ===============
 * Medications:
 *   - IX_Medications_RxCui (UNIQUE)
 *   - IX_Medications_Name
 *   - IX_Medications_GenericName
 *   - IX_Medications_IsOTC
 * 
 * UserMedications:
 *   - IX_UserMedications_MedicationId
 *   - IX_UserMedications_IsActive
 *   - IX_UserMedications_MedicationId_IsActive (COMPOSITE)
 * 
 * DrugInteractions:
 *   - IX_DrugInteractions_Drug1RxCui
 *   - IX_DrugInteractions_Drug2RxCui
 *   - IX_DrugInteractions_Drug1_Drug2_Unique (COMPOSITE, UNIQUE)
 *   - IX_DrugInteractions_SeverityLevel
 * 
 * MedicationSchedules:
 *   - IX_MedicationSchedules_UserMedicationId
 *   - IX_MedicationSchedules_IsActive
 *   - IX_MedicationSchedules_UserMedicationId_IsActive (COMPOSITE)
 *   - IX_MedicationSchedules_GeneratedDate
 * 
 * MedicationSets:
 *   - IX_MedicationSets_Name
 *   - IX_MedicationSets_CreatedDate
 *   - IX_MedicationSets_ModifiedDate
 * 
 * ActiveIngredientDuplicates:
 *   - IX_ActiveIngredientDuplicates_RxCui1
 *   - IX_ActiveIngredientDuplicates_RxCui2
 *   - IX_ActiveIngredientDuplicates_SharedIngredient
 *   - IX_ActiveIngredientDuplicates_Unique (COMPOSITE, UNIQUE)
 * 
 * AppSettings:
 *   - IX_AppSettings_SettingKey (UNIQUE)
 */