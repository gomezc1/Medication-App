using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicationManager.Core.Models;
using MedicationManager.Core.Models.Schedule;
using MedicationManager.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows;


namespace MedicationScheduler.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IMedicationService _medicationService;
        private readonly IScheduleService _scheduleService;
        private readonly IInteractionService _interactionService;
        private readonly ILogger<MainViewModel> _logger;

        #region Observable Properties

        [ObservableProperty]
        private ObservableCollection<UserMedication> _userMedications = new();

        [ObservableProperty]
        private DailySchedule? _currentSchedule;

        [ObservableProperty]
        private UserMedication? _selectedMedication;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private int _totalWarnings;

        [ObservableProperty]
        private string _currentView = "MedicationList";

        #endregion

        public MainViewModel(
            IMedicationService medicationService,
            IScheduleService scheduleService,
            IInteractionService interactionService,
            ILogger<MainViewModel> logger)
        {
            _medicationService = medicationService;
            _scheduleService = scheduleService;
            _interactionService = interactionService;
            _logger = logger;

            // Load initial data
            _ = LoadDataAsync();
        }

        #region Commands

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading medications...";

                var medications = await _medicationService.GetActiveUserMedicationsAsync();
                UserMedications = new ObservableCollection<UserMedication>(medications);

                if (UserMedications.Count > 0)
                {
                    await RegenerateScheduleAsync();
                }
                else
                {
                    StatusMessage = "No medications added yet. Click 'Add Medication' to get started.";
                }

                _logger.LogInformation("Loaded {Count} medications", UserMedications.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading medications");
                StatusMessage = "Error loading medications";
                MessageBox.Show(
                    $"Failed to load medications: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void NavigateToMedicationList()
        {
            CurrentView = "MedicationList";
            StatusMessage = "Viewing medication list";
        }

        [RelayCommand]
        private async Task NavigateToScheduleAsync()
        {
            CurrentView = "Schedule";
            StatusMessage = "Viewing daily schedule";

            if (CurrentSchedule == null && UserMedications.Count > 0)
            {
                await RegenerateScheduleAsync();
            }
        }

        [RelayCommand]
        private void NavigateToWarnings()
        {
            CurrentView = "Warnings";
            StatusMessage = "Viewing warnings and interactions";
        }

        [RelayCommand]
        private void NavigateToSettings()
        {
            CurrentView = "Settings";
            StatusMessage = "Viewing settings";
        }

        [RelayCommand]
        private async Task AddMedicationAsync()
        {
            try
            {
                // TODO: Open Add Medication dialog
                // For now, just refresh the list after dialog closes
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding medication");
                MessageBox.Show(
                    $"Failed to add medication: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task EditMedicationAsync(UserMedication? medication)
        {
            if (medication == null)
                return;

            try
            {
                SelectedMedication = medication;
                // TODO: Open Edit Medication dialog
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing medication");
                MessageBox.Show(
                    $"Failed to edit medication: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteMedicationAsync(UserMedication? medication)
        {
            if (medication == null)
                return;

            var result = MessageBox.Show(
                $"Are you sure you want to remove {medication.Medication.Name}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                IsLoading = true;
                StatusMessage = $"Removing {medication.Medication.Name}...";

                var success = await _medicationService.DeleteUserMedicationAsync(medication.Id);

                if (success)
                {
                    UserMedications.Remove(medication);
                    await RegenerateScheduleAsync();
                    StatusMessage = $"Removed {medication.Medication.Name}";

                    _logger.LogInformation("Deleted medication: {Name} (ID: {Id})",
                        medication.Medication.Name, medication.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting medication");
                MessageBox.Show(
                    $"Failed to delete medication: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RegenerateScheduleAsync()
        {
            if (UserMedications.Count == 0)
            {
                CurrentSchedule = null;
                TotalWarnings = 0;
                StatusMessage = "No medications to schedule";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Generating schedule...";

                var medications = UserMedications.ToList();
                CurrentSchedule = await _scheduleService.GenerateScheduleAsync(medications);

                TotalWarnings = CurrentSchedule.TotalIssues;

                StatusMessage = CurrentSchedule.HasWarnings
                    ? $"Schedule generated with {TotalWarnings} warning(s)"
                    : "Schedule generated successfully";

                _logger.LogInformation("Generated schedule with {Entries} entries and {Warnings} warnings",
                    CurrentSchedule.Entries.Count, TotalWarnings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating schedule");
                StatusMessage = "Error generating schedule";
                MessageBox.Show(
                    $"Failed to generate schedule: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ExportScheduleToPdfAsync()
        {
            if (CurrentSchedule == null)
            {
                MessageBox.Show(
                    "Please generate a schedule first.",
                    "No Schedule",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Exporting schedule to PDF...";

                var pdfBytes = await _scheduleService.ExportScheduleToPdfAsync(CurrentSchedule);

                // Save file dialog
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Medication_Schedule_{DateTime.Now:yyyy-MM-dd}",
                    DefaultExt = ".pdf",
                    Filter = "PDF Documents (.pdf)|*.pdf"
                };

                if (dialog.ShowDialog() == true)
                {
                    await System.IO.File.WriteAllBytesAsync(dialog.FileName, pdfBytes);

                    StatusMessage = "Schedule exported successfully";

                    var result = MessageBox.Show(
                        $"Schedule exported to:\n{dialog.FileName}\n\nWould you like to open it?",
                        "Export Successful",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = dialog.FileName,
                            UseShellExecute = true
                        });
                    }

                    _logger.LogInformation("Exported schedule to PDF: {FileName}", dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting schedule to PDF");
                MessageBox.Show(
                    $"Failed to export schedule: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveMedicationSetAsync()
        {
            if (UserMedications.Count == 0)
            {
                MessageBox.Show(
                    "No medications to save.",
                    "No Medications",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // TODO: Show dialog to get name and description
            var setName = $"Medication Set {DateTime.Now:yyyy-MM-dd HH:mm}";
            var description = "Saved medication set";

            try
            {
                IsLoading = true;
                StatusMessage = "Saving medication set...";

                var medications = UserMedications.ToList();
                var savedSet = await _medicationService.SaveMedicationSetAsync(
                    setName,
                    description,
                    medications);

                StatusMessage = "Medication set saved successfully";

                MessageBox.Show(
                    $"Saved medication set: {savedSet.Name}",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                _logger.LogInformation("Saved medication set: {Name} (ID: {Id})",
                    savedSet.Name, savedSet.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving medication set");
                MessageBox.Show(
                    $"Failed to save medication set: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoadMedicationSetAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading medication sets...";

                var sets = await _medicationService.GetSavedMedicationSetsAsync();

                if (sets.Count == 0)
                {
                    MessageBox.Show(
                        "No saved medication sets found.",
                        "No Sets",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                // TODO: Show dialog to select a set
                // For now, just load the first one as an example
                var setToLoad = sets[0];

                StatusMessage = $"Loading medication set: {setToLoad.Name}...";

                var medications = await _medicationService.LoadMedicationSetAsync(setToLoad.Id);

                UserMedications = new ObservableCollection<UserMedication>(medications);
                await RegenerateScheduleAsync();

                StatusMessage = $"Loaded medication set: {setToLoad.Name}";

                MessageBox.Show(
                    $"Loaded {medications.Count} medications from set: {setToLoad.Name}",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                _logger.LogInformation("Loaded medication set: {Name} (ID: {Id}) with {Count} medications",
                    setToLoad.Name, setToLoad.Id, medications.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading medication set");
                MessageBox.Show(
                    $"Failed to load medication set: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadDataAsync();
        }

        [RelayCommand]
        private void ShowAbout()
        {
            MessageBox.Show(
                "Medication Manager v1.0\n\n" +
                "⚠️ EDUCATIONAL PROTOTYPE\n\n" +
                "This application is for educational purposes only.\n" +
                "Always consult your healthcare provider before making\n" +
                "any changes to your medication regimen.\n\n" +
                "NOT FDA-approved • NOT HIPAA-compliant\n" +
                "NOT for actual medical decision-making",
                "About Medication Manager",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when a medication is added from a child view/dialog
        /// </summary>
        public async Task OnMedicationAddedAsync(UserMedication medication)
        {
            UserMedications.Add(medication);
            await RegenerateScheduleAsync();

            StatusMessage = $"Added {medication.Medication.Name}";

            _logger.LogInformation("Added medication: {Name} (ID: {Id})",
                medication.Medication.Name, medication.Id);
        }

        /// <summary>
        /// Called when a medication is updated from a child view/dialog
        /// </summary>
        public async Task OnMedicationUpdatedAsync(UserMedication medication)
        {
            var existing = UserMedications.FirstOrDefault(m => m.Id == medication.Id);
            if (existing != null)
            {
                var index = UserMedications.IndexOf(existing);
                UserMedications[index] = medication;
                await RegenerateScheduleAsync();

                StatusMessage = $"Updated {medication.Medication.Name}";

                _logger.LogInformation("Updated medication: {Name} (ID: {Id})",
                    medication.Medication.Name, medication.Id);
            }
        }

        #endregion
    }
}