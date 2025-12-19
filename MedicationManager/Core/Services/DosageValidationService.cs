using MedicationManager.Core.Models;
using MedicationManager.Core.Models.Configuration;
using MedicationManager.Core.Models.Warnings;
using MedicationManager.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace MedicationManager.Core.Services
{
    public class DosageValidationService : IDosageValidationService
    {
        private readonly ILogger<DosageValidationService> _logger;
        private readonly Dictionary<string, OtcDosageLimit> _otcLimits;

        public DosageValidationService(ILogger<DosageValidationService> logger)
        {
            _logger = logger;
            _otcLimits = InitializeOtcLimits();
        }

        public async Task<List<DosageWarning>> ValidateAllMedicationsAsync(List<UserMedication> medications)
        {
            var warnings = new List<DosageWarning>();

            foreach (var medication in medications.Where(m => m.IsActive))
            {
                var warning = await ValidateIndividualMedicationAsync(medication);
                if (warning != null)
                {
                    warnings.Add(warning);
                }
            }

            return warnings;
        }

        public async Task<DosageWarning?> ValidateIndividualMedicationAsync(UserMedication medication)
        {
            await Task.CompletedTask; // Async for future expansion

            if (!medication.Medication.IsOTC)
            {
                return new DosageWarning
                {
                    MedicationName = medication.Medication.Name,
                    CurrentDailyDose = medication.UserDose * medication.Frequency,
                    Unit = medication.UserDoseUnit,
                    Warning = "This is a prescription medication. Please ensure dosage matches your prescription.",
                    Level = WarningLevel.Info
                };
            }

            // Check OTC limits
            foreach (var ingredient in medication.Medication.ActiveIngredients)
            {
                var normalized = ingredient.ToLowerInvariant().Trim();
                if (_otcLimits.TryGetValue(normalized, out var limit))
                {
                    var dailyDose = medication.UserDose * medication.Frequency;
                    var convertedDose = ConvertToCommonUnit(dailyDose, medication.UserDoseUnit, limit.Unit);

                    if (convertedDose > limit.MaxDailyDose)
                    {
                        return new DosageWarning
                        {
                            MedicationName = medication.Medication.Name,
                            CurrentDailyDose = convertedDose,
                            MaxRecommendedDose = limit.MaxDailyDose,
                            Unit = limit.Unit,
                            Warning = $"Daily dose of {convertedDose:F1} {limit.Unit} exceeds maximum of {limit.MaxDailyDose:F0} {limit.Unit}. {limit.WarningMessage}",
                            Level = WarningLevel.Danger
                        };
                    }
                    else if (convertedDose > limit.MaxDailyDose * 0.8m)
                    {
                        return new DosageWarning
                        {
                            MedicationName = medication.Medication.Name,
                            CurrentDailyDose = convertedDose,
                            MaxRecommendedDose = limit.MaxDailyDose,
                            Unit = limit.Unit,
                            Warning = $"Daily dose of {convertedDose:F1} {limit.Unit} is approaching maximum of {limit.MaxDailyDose:F0} {limit.Unit}.",
                            Level = WarningLevel.Warning
                        };
                    }
                }
            }

            return null;
        }

        private decimal ConvertToCommonUnit(decimal amount, string fromUnit, string toUnit)
        {
            var normalizedFrom = fromUnit.ToLowerInvariant();
            var normalizedTo = toUnit.ToLowerInvariant();

            if (normalizedFrom == normalizedTo) return amount;

            var conversions = new Dictionary<(string, string), decimal>
            {
                [("g", "mg")] = 1000m,
                [("mg", "g")] = 0.001m,
                [("mg", "mcg")] = 1000m,
                [("mcg", "mg")] = 0.001m
            };

            return conversions.TryGetValue((normalizedFrom, normalizedTo), out var factor) ? amount * factor : amount;
        }

        private Dictionary<string, OtcDosageLimit> InitializeOtcLimits()
        {
            return new Dictionary<string, OtcDosageLimit>
            {
                ["acetaminophen"] = new() { IngredientName = "Acetaminophen", MaxDailyDose = 4000m, Unit = "mg", MaxDurationDays = 10, WarningMessage = "Exceeding this may cause liver damage." },
                ["ibuprofen"] = new() { IngredientName = "Ibuprofen", MaxDailyDose = 1200m, Unit = "mg", MaxDurationDays = 10, WarningMessage = "May increase risk of stomach bleeding." },
                ["naproxen"] = new() { IngredientName = "Naproxen", MaxDailyDose = 660m, Unit = "mg", MaxDurationDays = 10, WarningMessage = "May increase risk of stomach bleeding." },
                ["aspirin"] = new() { IngredientName = "Aspirin", MaxDailyDose = 4000m, Unit = "mg", MaxDurationDays = 10, WarningMessage = "High doses may cause stomach bleeding." }
            };
        }
    }
}