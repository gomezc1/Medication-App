using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicationManager.Core.Models;
using MedicationManager.Core.Models.DTOs;
using MedicationManager.Core.Services.Interfaces;
using MedicationScheduler.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace MedicationScheduler.ViewModels
{
    public partial class AddEditMedicationViewModel : ObservableObject
    {
        private readonly IMedicationService _medicationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<AddEditMedicationViewModel> _logger;

        #region Observable Properties

        [ObservableProperty]
        private string _searchTerm = string.Empty;

        [ObservableProperty]
        private ObservableCollection<MedicationSearchResult> _searchResults = new();

        [ObservableProperty]
        private MedicationSearchResult? _selectedSearchResult;

        [ObservableProperty]
        private Medication? _selectedMedication;

        [ObservableProperty]
        private decimal _dose = 1;

        [ObservableProperty]
        private string _doseUnit = "tablet";

        [ObservableProperty]
        private int _frequency = 1;

        [ObservableProperty]
        private ObservableCollection<TimingPreference> _selectedTimingPreferences = new();

        [ObservableProperty]
        private ObservableCollection<TimeSpan> _specificTimes = new();

        [ObservableProperty]
        private bool _withFood;

        [ObservableProperty]
        private bool _onEmptyStomach;

        [ObservableProperty]
        private string _specialInstructions = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private bool _useSpecificTimes;

        [ObservableProperty]
        private UserMedication? _result;

        private UserMedication? _originalMedication;

        #endregion

        #region Available Options

        public ObservableCollection<string> DoseUnits { get; } = new()
        {
            "tablet", "capsule", "mg", "ml", "g", "mcg", "IU", "drop", "spray", "patch", "suppository"
        };

        public ObservableCollection<TimingPreference> AvailableTimingPreferences { get; } = new()
        {
            TimingPreference.Morning,
            TimingPreference.Noon,
            TimingPreference.Evening,
            TimingPreference.Bedtime
        };

        public ObservableCollection<int> FrequencyOptions { get; } = new()
        {
            1, 2, 3, 4, 5, 6, 7, 8
        };

        #endregion

        public AddEditMedicationViewModel(
            IMedicationService medicationService,
            IDialogService dialogService,
            ILogger<AddEditMedicationViewModel> logger)
        {
            _medicationService = medicationService;
            _dialogService = dialogService;
            _logger = logger;
        }

        #region Commands

        [RelayCommand]
        private async Task SearchMedicationsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchTerm) || SearchTerm.Length < 2)
            {
                SearchResults.Clear();
                return;
            }

            try
            {
                IsLoading = true;
                var results = await _medicationService.SearchMedicationsAsync(SearchTerm);

                SearchResults.Clear();
                var chunk = results.Where(p => String.IsNullOrWhiteSpace(p.Medication.Name) == false).Take(20);
                foreach (var result in chunk)
                {
                    SearchResults.Add(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching medications");
                await _dialogService.ShowErrorAsync("Search Error", $"Failed to search medications: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SelectMedicationAsync(MedicationSearchResult? searchResult)
        {
            if (searchResult == null) return;

            try
            {
                IsLoading = true;

                // Get full medication details
                var medication = await _medicationService.GetMedicationByRxCuiAsync(searchResult.Medication.RxCui);

                if (medication != null)
                {
                    SelectedMedication = medication;
                    SelectedSearchResult = searchResult;

                    // Pre-fill some fields based on medication data
                    if (!string.IsNullOrEmpty(medication.DosageForm))
                    {
                        DoseUnit = medication.DosageForm.ToLower();
                    }

                    // Clear search results after selection
                    SearchResults.Clear();
                    SearchTerm = medication.Name;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting medication");
                await _dialogService.ShowErrorAsync("Error", $"Failed to load medication details: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void AddTimingPreference(TimingPreference preference)
        {
            if (!SelectedTimingPreferences.Contains(preference))
            {
                SelectedTimingPreferences.Add(preference);
            }
        }

        [RelayCommand]
        private void RemoveTimingPreference(TimingPreference preference)
        {
            SelectedTimingPreferences.Remove(preference);
        }

        [RelayCommand]
        private void AddSpecificTime()
        {
            // Add a default time (8:00 AM) - user can modify in UI
            var newTime = new TimeSpan(8, 0, 0);
            SpecificTimes.Add(newTime);
        }

        [RelayCommand]
        private void RemoveSpecificTime(TimeSpan time)
        {
            SpecificTimes.Remove(time);
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (!ValidateInput())
            {
                return;
            }

            try
            {
                IsLoading = true;

                if (IsEditMode && _originalMedication != null)
                {
                    // Update existing medication
                    var updateRequest = new UpdateUserMedicationRequest
                    {
                        Dose = Dose,
                        DoseUnit = DoseUnit,
                        Frequency = Frequency,
                        TimingPreferences = SelectedTimingPreferences.ToList(),
                        SpecificTimes = SpecificTimes.ToList(),
                        WithFood = WithFood,
                        OnEmptyStomach = OnEmptyStomach,
                        SpecialInstructions = SpecialInstructions,
                        IsActive = _originalMedication.IsActive
                    };

                    Result = await _medicationService.UpdateUserMedicationAsync(_originalMedication.Id, updateRequest);
                }
                else
                {
                    // Add new medication
                    if (SelectedMedication == null)
                    {
                        await _dialogService.ShowWarningAsync("Validation Error", "Please select a medication first.");
                        return;
                    }

                    var addRequest = new AddUserMedicationRequest
                    {
                        RxCui = SelectedMedication.RxCui,
                        Dose = Dose,
                        DoseUnit = DoseUnit,
                        Frequency = Frequency,
                        TimingPreferences = SelectedTimingPreferences.ToList(),
                        SpecificTimes = SpecificTimes.ToList(),
                        WithFood = WithFood,
                        OnEmptyStomach = OnEmptyStomach,
                        SpecialInstructions = SpecialInstructions,
                        StartDate = DateTime.Now
                    };

                    Result = await _medicationService.AddUserMedicationAsync(addRequest);
                }

                _logger.LogInformation("Medication {Action} successfully", IsEditMode ? "updated" : "added");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving medication");
                await _dialogService.ShowErrorAsync("Save Error", $"Failed to save medication: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            // Set Result to null to indicate cancellation
            Result = null;

            // Raise property changed to trigger dialog close
            OnPropertyChanged(nameof(Result));
        }

        #endregion

        #region Public Methods

        public async Task LoadMedicationForEditAsync(UserMedication medication)
        {
            _originalMedication = medication;
            IsEditMode = true;

            // Load medication details
            SelectedMedication = medication.Medication;
            SearchTerm = medication.Medication.Name;

            // Load dosage info
            Dose = medication.UserDose;
            DoseUnit = medication.UserDoseUnit;
            Frequency = medication.Frequency;

            // Load timing preferences
            SelectedTimingPreferences.Clear();
            if (medication.TimingPreferences != null)
            {
                foreach (var pref in medication.TimingPreferences)
                {
                    SelectedTimingPreferences.Add(pref);
                }
            }

            // Load specific times
            SpecificTimes.Clear();
            if (medication.SpecificTimes != null)
            {
                foreach (var time in medication.SpecificTimes)
                {
                    SpecificTimes.Add(time);
                }
            }

            UseSpecificTimes = SpecificTimes.Any();

            // Load special instructions
            WithFood = medication.WithFood;
            OnEmptyStomach = medication.OnEmptyStomach;
            SpecialInstructions = medication.SpecialInstructions ?? string.Empty;

            await Task.CompletedTask;
        }

        #endregion

        #region Helper Methods

        private bool ValidateInput()
        {
            if (!IsEditMode && SelectedMedication == null)
            {
                _dialogService.ShowWarningAsync("Validation Error", "Please select a medication.");
                return false;
            }

            if (Dose <= 0)
            {
                _dialogService.ShowWarningAsync("Validation Error", "Dose must be greater than 0.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(DoseUnit))
            {
                _dialogService.ShowWarningAsync("Validation Error", "Please specify a dose unit.");
                return false;
            }

            if (Frequency < 1 || Frequency > 8)
            {
                _dialogService.ShowWarningAsync("Validation Error", "Frequency must be between 1 and 8 times per day.");
                return false;
            }

            if (UseSpecificTimes)
            {
                if (!SpecificTimes.Any())
                {
                    _dialogService.ShowWarningAsync("Validation Error", "Please add at least one specific time.");
                    return false;
                }

                if (SpecificTimes.Count != Frequency)
                {
                    _dialogService.ShowWarningAsync("Validation Error",
                        $"Number of specific times ({SpecificTimes.Count}) must match frequency ({Frequency}).");
                    return false;
                }
            }
            else
            {
                if (!SelectedTimingPreferences.Any())
                {
                    _dialogService.ShowWarningAsync("Validation Error", "Please select at least one timing preference.");
                    return false;
                }
            }

            if (WithFood && OnEmptyStomach)
            {
                _dialogService.ShowWarningAsync("Validation Error",
                    "Medication cannot be both 'with food' and 'on empty stomach'.");
                return false;
            }

            return true;
        }

        partial void OnSearchTermChanged(string value)
        {
            // Auto-search when user types
            if (value.Length >= 2)
            {
                _ = SearchMedicationsAsync();
            }
            else if (value.Length == 0)
            {
                SearchResults.Clear();
            }
        }

        partial void OnUseSpecificTimesChanged(bool value)
        {
            if (value)
            {
                // Clear timing preferences when switching to specific times
                SelectedTimingPreferences.Clear();

                // Add default times based on frequency if none exist
                if (!SpecificTimes.Any() && Frequency > 0)
                {
                    for (int i = 0; i < Frequency; i++)
                    {
                        SpecificTimes.Add(new TimeSpan(8 + (i * 4), 0, 0));
                    }
                }
            }
            else
            {
                // Clear specific times when switching to timing preferences
                SpecificTimes.Clear();
            }
        }

        partial void OnFrequencyChanged(int value)
        {
            // Adjust specific times if using specific times mode
            if (UseSpecificTimes)
            {
                while (SpecificTimes.Count < value)
                {
                    var lastTime = SpecificTimes.LastOrDefault();
                    var newTime = lastTime != default ? lastTime.Add(TimeSpan.FromHours(4)) : new TimeSpan(8, 0, 0);
                    SpecificTimes.Add(newTime);
                }

                while (SpecificTimes.Count > value)
                {
                    SpecificTimes.RemoveAt(SpecificTimes.Count - 1);
                }
            }
        }

        #endregion
    }
}