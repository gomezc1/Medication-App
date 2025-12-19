using MedicationManager.Core.Models;
using MedicationManager.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace MedicationManager.Core.Services
{
    public class DataSeedService : IDataSeedService
    {
        private readonly IRepository<Medication> _medicationRepository;
        private readonly IRepository<DrugInteraction> _interactionRepository;
        private readonly ILogger<DataSeedService> _logger;

        public DataSeedService(
            IRepository<Medication> medicationRepository,
            IRepository<DrugInteraction> interactionRepository,
            ILogger<DataSeedService> logger)
        {
            _medicationRepository = medicationRepository;
            _interactionRepository = interactionRepository;
            _logger = logger;
        }

        public async Task SeedInitialDataAsync()
        {
            var count = await _medicationRepository.CountAsync();
            if (count > 0)
            {
                _logger.LogInformation("Database already seeded, skipping");
                return;
            }

            _logger.LogInformation("Seeding initial data");

            await SeedOtcMedicationsAsync();
            await SeedInteractionDataAsync();

            _logger.LogInformation("Initial data seeded successfully");
        }

        public async Task SeedOtcMedicationsAsync()
        {
            var medications = new List<Medication>
            {
                new() { RxCui = "161", Name = "Tylenol", GenericName = "Acetaminophen", ActiveIngredients = new() { "Acetaminophen" }, Strength = "500mg", DosageForm = "tablet", IsOTC = true, MaxDailyDose = 4000, MaxDailyDoseUnit = "mg", DataSource = "Seed" },
                new() { RxCui = "5640", Name = "Advil", GenericName = "Ibuprofen", ActiveIngredients = new() { "Ibuprofen" }, Strength = "200mg", DosageForm = "tablet", IsOTC = true, MaxDailyDose = 1200, MaxDailyDoseUnit = "mg", DataSource = "Seed" },
                new() { RxCui = "1049502", Name = "Claritin", GenericName = "Loratadine", ActiveIngredients = new() { "Loratadine" }, Strength = "10mg", DosageForm = "tablet", IsOTC = true, MaxDailyDose = 10, MaxDailyDoseUnit = "mg", DataSource = "Seed" }
            };

            foreach (var med in medications)
            {
                await _medicationRepository.AddAsync(med);
            }

            _logger.LogInformation("Seeded {Count} OTC medications", medications.Count);
        }

        public async Task SeedInteractionDataAsync()
        {
            var interactions = new List<DrugInteraction>
            {
                new() { Drug1RxCui = "161", Drug2RxCui = "5640", SeverityLevel = InteractionSeverity.Minor, Description = "Both reduce pain and fever. Using together may provide enhanced relief.", Source = "Seed Data" }
            };

            foreach (var interaction in interactions)
            {
                await _interactionRepository.AddAsync(interaction);
            }

            _logger.LogInformation("Seeded {Count} drug interactions", interactions.Count);
        }
    }
}
