using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicationManager.Core.Models;
using MedicationManager.Core.Models.DTOs;
using MedicationManager.Core.Services.Interfaces;
using MedicationScheduler.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace MedicationScheduler.ViewModels
{
    public partial class InteractionWarningsViewModel : ObservableObject
    {
        private readonly IMedicationService _medicationService;
        private readonly IInteractionService _interactionService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<InteractionWarningsViewModel> _logger;

        #region Observable Properties

        [ObservableProperty]
        private ObservableCollection<MedicationWarningViewModel> _medications = new();

        [ObservableProperty]
        private InteractionCheckResult? _interactionResult;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _hasWarnings;

        [ObservableProperty]
        private int _totalIssues;

        [ObservableProperty]
        private int _drugInteractionCount;

        [ObservableProperty]
        private int _duplicateWarningCount;

        [ObservableProperty]
        private int _dosageWarningCount;

        #endregion

        public InteractionWarningsViewModel(
            IMedicationService medicationService,
            IInteractionService interactionService,
            IDialogService dialogService,
            ILogger<InteractionWarningsViewModel> logger)
        {
            _medicationService = medicationService;
            _interactionService = interactionService;
            _dialogService = dialogService;
            _logger = logger;
        }

        #region Commands

        [RelayCommand]
        public async Task LoadWarningsAsync()
        {
            try
            {
                IsLoading = true;

                // Get all active medications
                var userMedications = await _medicationService.GetAllUserMedicationsAsync();
                var activeMedications = userMedications.Where(m => m.IsActive).ToList();

                if (!activeMedications.Any())
                {
                    Medications.Clear();
                    HasWarnings = false;
                    TotalIssues = 0;
                    return;
                }

                // Check for interactions
                InteractionResult = await _interactionService.CheckInteractionsAsync(activeMedications);

                // Convert to view models
                Medications.Clear();
                foreach (var medication in activeMedications)
                {
                    var viewModel = new MedicationWarningViewModel
                    {
                        Name = medication.Medication.Name,
                        GenericName = medication.Medication.GenericName,
                        Dosage = $"{medication.UserDose} {medication.UserDoseUnit}",
                        Frequency = $"{medication.Frequency}x daily",
                        AccentBrush = GetAccentBrush(medication),

                        // Get interactions for this medication
                        DrugInteractions = InteractionResult.DrugInteractions
                            .Where(di => di.Drug1RxCui == medication.Medication.RxCui ||
                                        di.Drug2RxCui == medication.Medication.RxCui)
                            .ToList(),

                        // Get duplicate warnings for this medication
                        DuplicateWarnings = InteractionResult.DuplicateWarnings
                            .Where(dw => dw.MedicationNames.Contains(medication.Medication.Name))
                            .ToList(),

                        // Static warnings (these would come from FDA data in production)
                        Contraindications = GetContraindications(medication),
                        WarningsPrecautions = GetWarningsPrecautions(medication),
                        CommonInteractions = GetCommonInteractions(medication)
                    };

                    Medications.Add(viewModel);
                }

                // Update summary counts
                DrugInteractionCount = InteractionResult.DrugInteractions.Count;
                DuplicateWarningCount = InteractionResult.DuplicateWarnings.Count;
                DosageWarningCount = InteractionResult.DosageWarnings?.Count ?? 0;
                TotalIssues = DrugInteractionCount + DuplicateWarningCount + DosageWarningCount;
                HasWarnings = TotalIssues > 0;

                _logger.LogInformation("Loaded warnings for {Count} medications with {Issues} total issues",
                    Medications.Count, TotalIssues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading warnings");
                await _dialogService.ShowErrorAsync("Error", $"Failed to load warnings: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshWarningsAsync()
        {
            await LoadWarningsAsync();
        }

        #endregion

        #region Helper Methods

        private Brush GetAccentBrush(UserMedication medication)
        {
            // Get highest severity from interactions
            var highestSeverity = InteractionResult?.DrugInteractions
                .Where(di => di.Drug1RxCui == medication.Medication.RxCui ||
                            di.Drug2RxCui == medication.Medication.RxCui)
                .Select(di => di.SeverityLevel)
                .DefaultIfEmpty(InteractionSeverity.Minor)
                .Max() ?? InteractionSeverity.Minor;

            return highestSeverity switch
            {
                InteractionSeverity.Major => new SolidColorBrush(Color.FromRgb(220, 38, 38)),    // Red
                InteractionSeverity.Moderate => new SolidColorBrush(Color.FromRgb(251, 146, 60)), // Orange
                InteractionSeverity.Minor => new SolidColorBrush(Color.FromRgb(234, 179, 8)),    // Yellow
                _ => new SolidColorBrush(Color.FromRgb(34, 197, 94))                             // Green
            };
        }

        private ObservableCollection<string> GetContraindications(UserMedication medication)
        {
            // In production, these would come from FDA API or database
            // For now, return generic contraindications based on drug class
            var contraindications = new ObservableCollection<string>();

            // Example generic contraindications
            contraindications.Add("History of allergic reactions to this medication or similar drugs");
            contraindications.Add("Severe kidney or liver disease (consult healthcare provider)");
            contraindications.Add("Pregnancy or breastfeeding (consult healthcare provider)");

            return contraindications;
        }

        private ObservableCollection<string> GetWarningsPrecautions(UserMedication medication)
        {
            // In production, these would come from FDA API or database
            var warnings = new ObservableCollection<string>();

            // Example generic warnings
            warnings.Add("May cause dizziness or drowsiness. Use caution when driving or operating machinery.");
            warnings.Add("Avoid alcohol consumption while taking this medication.");
            warnings.Add("Monitor for unusual side effects and report to healthcare provider.");
            warnings.Add("Take as prescribed. Do not adjust dosage without consulting healthcare provider.");

            return warnings;
        }

        private ObservableCollection<string> GetCommonInteractions(UserMedication medication)
        {
            // In production, these would come from FDA API or database
            var interactions = new ObservableCollection<string>();

            // Add actual detected interactions
            if (InteractionResult != null)
            {
                var medInteractions = InteractionResult.DrugInteractions
                    .Where(di => di.Drug1RxCui == medication.Medication.RxCui ||
                                di.Drug2RxCui == medication.Medication.RxCui)
                    .ToList();

                foreach (var interaction in medInteractions)
                {
                    var otherDrug = interaction.Drug1RxCui == medication.Medication.RxCui
                        ? interaction.Drug2Name
                        : interaction.Drug1Name;

                    interactions.Add($"{otherDrug}: {interaction.Description}");
                }
            }

            // Add generic interaction warnings if no specific ones found
            if (!interactions.Any())
            {
                interactions.Add("NSAIDs (e.g., ibuprofen, naproxen): May interact with many medications");
                interactions.Add("Blood thinners: Consult healthcare provider about potential interactions");
                interactions.Add("Other prescription medications: Always inform your doctor of all medications");
            }

            return interactions;
        }

        #endregion
    }
}
