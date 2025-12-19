// ============================================================================
// File: MedicationManager.WPF/ViewModels/MedicationListViewModel.cs
// Description: View model for medication list management
// ============================================================================

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
    public partial class MedicationListViewModel : ObservableObject
    {
        private readonly IMedicationService _medicationService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<MedicationListViewModel> _logger;

        #region Observable Properties

        [ObservableProperty]
        private ObservableCollection<UserMedication> _medications = new();

        [ObservableProperty]
        private UserMedication? _selectedMedication;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private int _totalMedications;

        [ObservableProperty]
        private int _activeMedications;

        #endregion

        public MedicationListViewModel(
            IMedicationService medicationService,
            INavigationService navigationService,
            IDialogService dialogService,
            ILogger<MedicationListViewModel> logger)
        {
            _medicationService = medicationService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _logger = logger;
        }

        #region Commands

        [RelayCommand]
        private async Task LoadMedicationsAsync()
        {
            try
            {
                IsLoading = true;

                // FIXED: Use GetAllUserMedicationsAsync() instead of GetUserMedicationsAsync()
                var medications = await _medicationService.GetAllUserMedicationsAsync();

                Medications.Clear();
                foreach (var medication in medications.OrderBy(m => m.Medication.Name))
                {
                    Medications.Add(medication);
                }

                UpdateStatistics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading medications");
                await _dialogService.ShowErrorAsync("Error", $"Failed to load medications: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task AddMedicationAsync()
        {
            try
            {
                // NavigateToAddEditMedicationAsync returns Task<UserMedication?>, not just a navigation action
                var result = await _navigationService.NavigateToAddEditMedicationAsync();
                if (result != null)
                {
                    // Add the new medication to the list
                    Medications.Add(result);
                    UpdateStatistics();
                    await _dialogService.ShowSuccessAsync("Success", "Medication added successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding medication");
                await _dialogService.ShowErrorAsync("Error", $"Failed to add medication: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task EditMedicationAsync(UserMedication? medication)
        {
            if (medication == null) return;

            try
            {
                // Pass the medication to edit
                var result = await _navigationService.NavigateToAddEditMedicationAsync(medication);
                if (result != null)
                {
                    var index = Medications.IndexOf(medication);
                    if (index >= 0)
                    {
                        Medications[index] = result;
                        UpdateStatistics();
                        await _dialogService.ShowSuccessAsync("Success", "Medication updated successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing medication");
                await _dialogService.ShowErrorAsync("Error", $"Failed to edit medication: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task DeleteMedicationAsync(UserMedication? medication)
        {
            if (medication == null) return;

            try
            {
                var confirmed = await _dialogService.ShowConfirmationAsync(
                    "Delete Medication",
                    $"Are you sure you want to delete {medication.Medication.Name}?");

                if (confirmed)
                {
                    // FIXED: DeleteUserMedicationAsync returns Task<bool>, not Task
                    var deleted = await _medicationService.DeleteUserMedicationAsync(medication.Id);

                    if (deleted)
                    {
                        Medications.Remove(medication);
                        UpdateStatistics();
                        await _dialogService.ShowSuccessAsync("Success", "Medication deleted successfully");
                    }
                    else
                    {
                        await _dialogService.ShowWarningAsync("Warning", "Medication could not be deleted");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting medication");
                await _dialogService.ShowErrorAsync("Error", $"Failed to delete medication: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ToggleActiveStatusAsync(UserMedication? medication)
        {
            if (medication == null) return;

            try
            {
                // Store the original state in case we need to revert
                var originalState = medication.IsActive;

                // Toggle the status
                medication.IsActive = !medication.IsActive;

                // FIXED: UpdateUserMedicationAsync requires (int id, UpdateUserMedicationRequest request)
                // We need to create an UpdateUserMedicationRequest object
                var updateRequest = new UpdateUserMedicationRequest
                {
                    Dose = medication.UserDose,
                    DoseUnit = medication.UserDoseUnit,
                    Frequency = medication.Frequency,
                    TimingPreferences = medication.TimingPreferences,
                    SpecificTimes = medication.SpecificTimes,
                    WithFood = medication.WithFood,
                    OnEmptyStomach = medication.OnEmptyStomach,
                    SpecialInstructions = medication.SpecialInstructions,
                    IsActive = medication.IsActive
                };

                var updatedMedication = await _medicationService.UpdateUserMedicationAsync(medication.Id, updateRequest);

                // Update the item in the collection
                var index = Medications.IndexOf(medication);
                if (index >= 0)
                {
                    Medications[index] = updatedMedication;
                    UpdateStatistics();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling medication status");
                // Revert the change on error
                medication.IsActive = !medication.IsActive;
                await _dialogService.ShowErrorAsync("Error", $"Failed to update medication: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SearchMedicationsAsync()
        {
            try
            {
                IsLoading = true;

                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    // Load all medications if search is empty
                    await LoadMedicationsAsync();
                    return;
                }

                // FIXED: Use GetAllUserMedicationsAsync() and filter client-side
                var allMedications = await _medicationService.GetAllUserMedicationsAsync();
                var filtered = allMedications.Where(m =>
                    m.Medication.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (m.Medication.GenericName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)
                ).OrderBy(m => m.Medication.Name).ToList();

                Medications.Clear();
                foreach (var medication in filtered)
                {
                    Medications.Add(medication);
                }

                UpdateStatistics();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching medications");
                await _dialogService.ShowErrorAsync("Error", $"Failed to search medications: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ClearSearchAsync()
        {
            SearchText = string.Empty;
            await LoadMedicationsAsync();
        }

        [RelayCommand]
        private async Task SaveMedicationSetAsync()
        {
            try
            {
                if (!Medications.Any())
                {
                    await _dialogService.ShowWarningAsync("No Medications", "There are no medications to save.");
                    return;
                }

                var setName = await _dialogService.ShowInputAsync(
                    "Save Medication Set",
                    "Enter a name for this medication set:");

                if (!string.IsNullOrWhiteSpace(setName))
                {
                    var description = await _dialogService.ShowInputAsync(
                        "Set Description",
                        "Enter a description (optional):");

                    // FIXED: SaveMedicationSetAsync requires (string name, string description, List<UserMedication> medications)
                    await _medicationService.SaveMedicationSetAsync(setName, description ?? "", Medications.ToList());
                    await _dialogService.ShowSuccessAsync("Success", "Medication set saved successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving medication set");
                await _dialogService.ShowErrorAsync("Error", $"Failed to save medication set: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task LoadMedicationSetAsync()
        {
            try
            {
                var sets = await _medicationService.GetSavedMedicationSetsAsync();

                if (!sets.Any())
                {
                    await _dialogService.ShowInformationAsync("No Sets", "No saved medication sets found.");
                    return;
                }

                var selectedSet = await _dialogService.ShowSelectionAsync(
                    "Load Medication Set",
                    "Select a medication set:",
                    sets);

                if (selectedSet != null)
                {
                    var confirmed = await _dialogService.ShowConfirmationAsync(
                        "Replace Medications",
                        "Loading this set will replace your current medications. Continue?");

                    if (confirmed)
                    {
                        var medications = await _medicationService.LoadMedicationSetAsync(selectedSet.Id);

                        Medications.Clear();
                        foreach (var medication in medications.OrderBy(m => m.Medication.Name))
                        {
                            Medications.Add(medication);
                        }

                        UpdateStatistics();
                        await _dialogService.ShowSuccessAsync("Success", "Medication set loaded successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading medication set");
                await _dialogService.ShowErrorAsync("Error", $"Failed to load medication set: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadMedicationsAsync();
        }

        #endregion

        #region Helper Methods

        private void UpdateStatistics()
        {
            TotalMedications = Medications.Count;
            ActiveMedications = Medications.Count(m => m.IsActive);
        }

        partial void OnSearchTextChanged(string value)
        {
            // Automatically trigger search when search text changes
            // Using async void here is acceptable for event handlers
            _ = SearchMedicationsAsync();
        }

        #endregion
    }
}