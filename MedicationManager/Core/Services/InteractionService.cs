using MedicationManager.Core.Models;
using MedicationManager.Core.Models.DTOs;
using MedicationManager.Core.Models.Warnings;
using MedicationManager.Core.Services.Interfaces;
using MedicationManager.Infrastructure.ExternalServices.Interfaces;
using Microsoft.Extensions.Logging;

namespace MedicationManager.Core.Services
{
    public class InteractionService : IInteractionService
    {
        private readonly IRepository<DrugInteraction> _interactionRepository;
        private readonly IOpenFdaApiService _openFdaService;
        private readonly IRxNormApiService _rxNormService;
        private readonly ILogger<InteractionService> _logger;

        public InteractionService(
            IRepository<DrugInteraction> interactionRepository,
            IOpenFdaApiService openFdaService,
            IRxNormApiService rxNormService,
            ILogger<InteractionService> logger)
        {
            _interactionRepository = interactionRepository;
            _openFdaService = openFdaService;
            _rxNormService = rxNormService;
            _logger = logger;
        }

        public async Task<InteractionCheckResult> CheckInteractionsAsync(List<UserMedication> medications)
        {
            _logger.LogInformation("Checking interactions for {Count} medications", medications.Count);

            var result = new InteractionCheckResult
            {
                CheckedDate = DateTime.Now,
                CheckedMedications = medications.Select(m => m.Medication.Name).ToList()
            };

            // Check drug-to-drug interactions
            var drugInteractions = await CheckDrugToDrugInteractionsAsync(medications);
            result.DrugInteractions.AddRange(drugInteractions);

            // Check for duplicate active ingredients
            var duplicateWarnings = await CheckDuplicateIngredientsAsync(medications);
            result.DuplicateWarnings.AddRange(duplicateWarnings);

            _logger.LogInformation("Found {Total} total issues ({Drug} interactions, {Dup} duplicates)",
                result.TotalIssues, drugInteractions.Count, duplicateWarnings.Count);

            return result;
        }

        public async Task<List<DrugInteraction>> CheckDrugToDrugInteractionsAsync(List<UserMedication> medications)
        {
            var interactions = new List<DrugInteraction>();
            var names = medications.Where(m => !string.IsNullOrEmpty(m.Medication.RxCui))
                                   .Select(m => m.Medication.Name)
                                   .ToList();

            // Check all pairs of medications
            for (int i = 0; i < names.Count; i++)
            {
                for (int j = i + 1; j < names.Count; j++)
                {
                    var name1 = names[i];
                    var name2 = names[j];

                    // Check local database first
                    var localInteraction = await _interactionRepository.FindFirstAsync(di =>
                        (di.Drug1Name == name1 && di.Drug2Name == name2) ||
                        (di.Drug1Name == name2 && di.Drug2RxCui == name1));

                    if (localInteraction != null)
                    {
                        // Populate medication names
                        localInteraction.Drug1Name = medications.First(m => m.Medication.RxCui == localInteraction.Drug1RxCui).Medication.Name;
                        localInteraction.Drug2Name = medications.First(m => m.Medication.RxCui == localInteraction.Drug2RxCui).Medication.Name;
                        interactions.Add(localInteraction);
                    }
                    else
                    {
                        // Try to find from APIs
                        var apiInteraction = await FetchInteractionFromApiAsync(name1, name2, medications);
                        if (apiInteraction != null)
                        {
                            // Save for future use
                            await _interactionRepository.AddAsync(apiInteraction);
                            interactions.Add(apiInteraction);
                        }
                    }
                }
            }

            return interactions;
        }

        public async Task<List<DrugInteraction>> CheckDrugToDrugInteractionsAsync2(List<UserMedication> medications)
        {
            var interactions = new List<DrugInteraction>();
            var rxCuis = medications.Where(m => !string.IsNullOrEmpty(m.Medication.RxCui))
                                   .Select(m => m.Medication.RxCui)
                                   .ToList();

            // Check all pairs of medications
            for (int i = 0; i < rxCuis.Count; i++)
            {
                for (int j = i + 1; j < rxCuis.Count; j++)
                {
                    var rxCui1 = rxCuis[i];
                    var rxCui2 = rxCuis[j];

                    // Check local database first
                    var localInteraction = await _interactionRepository.FindFirstAsync(di =>
                        (di.Drug1RxCui == rxCui1 && di.Drug2RxCui == rxCui2) ||
                        (di.Drug1RxCui == rxCui2 && di.Drug2RxCui == rxCui1));

                    if (localInteraction != null)
                    {
                        // Populate medication names
                        localInteraction.Drug1Name = medications.First(m => m.Medication.RxCui == localInteraction.Drug1RxCui).Medication.Name;
                        localInteraction.Drug2Name = medications.First(m => m.Medication.RxCui == localInteraction.Drug2RxCui).Medication.Name;
                        interactions.Add(localInteraction);
                    }
                    else
                    {
                        // Try to find from APIs
                        var apiInteraction = await FetchInteractionFromApiAsync(rxCui1, rxCui2, medications);
                        if (apiInteraction != null)
                        {
                            // Save for future use
                            await _interactionRepository.AddAsync(apiInteraction);
                            interactions.Add(apiInteraction);
                        }
                    }
                }
            }

            return interactions;
        }

        public async Task<List<DuplicateActiveIngredientWarning>> CheckDuplicateIngredientsAsync(List<UserMedication> medications)
        {
            var warnings = new List<DuplicateActiveIngredientWarning>();
            var ingredientGroups = new Dictionary<string, List<UserMedication>>();

            // Group medications by active ingredient
            foreach (var medication in medications)
            {
                foreach (var ingredient in medication.Medication.ActiveIngredients)
                {
                    var normalized = ingredient.ToLowerInvariant().Trim();
                    if (!ingredientGroups.ContainsKey(normalized))
                    {
                        ingredientGroups[normalized] = new List<UserMedication>();
                    }
                    ingredientGroups[normalized].Add(medication);
                }
            }

            // Check for duplicates
            foreach (var group in ingredientGroups.Where(g => g.Value.Count > 1))
            {
                var ingredient = group.Key;
                var meds = group.Value;

                var totalDailyDose = meds.Sum(m => m.UserDose * m.Frequency);
                var unit = meds.First().UserDoseUnit;

                warnings.Add(new DuplicateActiveIngredientWarning
                {
                    ActiveIngredient = FormatIngredientName(ingredient),
                    MedicationNames = meds.Select(m => m.Medication.Name).Distinct().ToList(),
                    TotalDailyDose = totalDailyDose,
                    Unit = unit,
                    Warning = $"You are taking {meds.Count} medications containing {FormatIngredientName(ingredient)}, totaling {totalDailyDose:F1} {unit} per day. Please consult your healthcare provider."
                });
            }

            return warnings;
        }

        private async Task<DrugInteraction?> FetchInteractionFromApiAsync(string name1, string name2, List<UserMedication> medications)
        {
            try
            {
                //var interactions = await _openFdaService.GetDrugInteractionsAsync(rxCui1);
                var interactions = await _openFdaService.GetDrugInteractionsByNameAsync(name1, name2);
                var result = interactions.FirstOrDefault();
                if (result != null)
                {
                    var rxCui1 = medications.First(m => m.Medication.Name == name1).Medication.RxCui;
                    var rxCui2 = medications.First(m => m.Medication.Name == name2).Medication.RxCui;

                    result.Drug1RxCui = rxCui1;
                    result.Drug2RxCui = rxCui2;
                    result.Drug1Name = name1;
                    result.Drug2Name = name2;

                    return result;
                }

                //return interactions.FirstOrDefault();

                //var med1Name = medications.First(m => m.Medication.RxCui == rxCui1).Medication.Name;
                //var med2Name = medications.First(m => m.Medication.RxCui == rxCui2).Medication.Name;

                //// Look for interactions mentioning the second drug
                //var relevantInteraction = interactions.FirstOrDefault(i =>
                //    i.Description.Contains(med2Name, StringComparison.OrdinalIgnoreCase));

                //if (relevantInteraction != null)
                //{
                //    relevantInteraction.Drug1RxCui = rxCui1;
                //    relevantInteraction.Drug2RxCui = rxCui2;
                //    relevantInteraction.Drug1Name = med1Name;
                //    relevantInteraction.Drug2Name = med2Name;
                //    return relevantInteraction;
                //}
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch interaction between {Name1} and {Name2}", name1, name2);
            }

            return null;
        }

        private string FormatIngredientName(string ingredient)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ingredient.ToLower());
        }
    }
}