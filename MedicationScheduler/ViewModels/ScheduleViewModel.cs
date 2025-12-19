using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicationManager.Core.Models.Schedule;
using MedicationManager.Core.Services.Interfaces;
using MedicationScheduler.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace MedicationScheduler.ViewModels
{
    public partial class ScheduleViewModel : ObservableObject
    {
        private readonly IMedicationService _medicationService;
        private readonly IScheduleService _scheduleService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ScheduleViewModel> _logger;

        #region Observable Properties

        [ObservableProperty]
        private ObservableCollection<ScheduleItemViewModel> _scheduleItems = new();

        [ObservableProperty]
        private DailySchedule? _currentSchedule;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _hasSchedule;

        [ObservableProperty]
        private int _totalMedications;

        [ObservableProperty]
        private int _totalDoses;

        [ObservableProperty]
        private string _scheduleDate = DateTime.Today.ToString("MMMM dd, yyyy");

        #endregion

        public ScheduleViewModel(
            IMedicationService medicationService,
            IScheduleService scheduleService,
            IDialogService dialogService,
            ILogger<ScheduleViewModel> logger)
        {
            _medicationService = medicationService;
            _scheduleService = scheduleService;
            _dialogService = dialogService;
            _logger = logger;
        }

        #region Commands

        [RelayCommand]
        public async Task LoadScheduleAsync()
        {
            try
            {
                IsLoading = true;

                // Get active medications
                var medications = await _medicationService.GetAllUserMedicationsAsync();
                var activeMedications = medications.Where(m => m.IsActive).ToList();

                if (!activeMedications.Any())
                {
                    HasSchedule = false;
                    ScheduleItems.Clear();
                    TotalMedications = 0;
                    TotalDoses = 0;
                    return;
                }

                // Generate schedule
                CurrentSchedule = await _scheduleService.GenerateScheduleAsync(activeMedications);

                // Convert to view models
                ScheduleItems.Clear();
                foreach (var entry in CurrentSchedule.Entries.OrderBy(e => e.Time))
                {
                    // Each ScheduleEntry can have multiple medications
                    foreach (var dose in entry.Medications)
                    {
                        ScheduleItems.Add(new ScheduleItemViewModel
                        {
                            TimeLabel = entry.FormattedTime,
                            MedicationName = dose.Medication.Medication.Name,
                            Dosage = $"{dose.Amount} {dose.Unit}",
                            Instructions = GetInstructions(dose),
                            AccentBrush = GetAccentBrush(entry.Time),
                            ScheduledTime = entry.Time,
                            TimeSlot = entry.TimeSlot,
                            UserMedication = dose.Medication
                        });
                    }
                }

                HasSchedule = ScheduleItems.Any();
                TotalMedications = activeMedications.Count;
                TotalDoses = ScheduleItems.Count;

                _logger.LogInformation("Schedule loaded with {Count} entries", ScheduleItems.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading schedule");
                await _dialogService.ShowErrorAsync("Error", $"Failed to load schedule: {ex.Message}");
                HasSchedule = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshScheduleAsync()
        {
            await LoadScheduleAsync();
        }

        [RelayCommand]
        private async Task ExportScheduleAsync()
        {
            if (CurrentSchedule == null)
            {
                await _dialogService.ShowWarningAsync("No Schedule", "Please load a schedule first.");
                return;
            }

            try
            {
                IsLoading = true;

                var filePath = await _dialogService.ShowSaveFileDialogAsync(
                    "Export Schedule",
                    "PDF Files (*.pdf)|*.pdf",
                    $"MedicationSchedule_{DateTime.Today:yyyyMMdd}.pdf");

                if (string.IsNullOrEmpty(filePath))
                {
                    return; // User cancelled
                }

                var pdfBytes = await _scheduleService.ExportScheduleToPdfAsync(CurrentSchedule);
                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

                await _dialogService.ShowSuccessAsync(
                    "Export Successful",
                    $"Schedule exported to:\n{filePath}");

                _logger.LogInformation("Schedule exported to {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting schedule");
                await _dialogService.ShowErrorAsync("Export Error", $"Failed to export schedule: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Helper Methods

        private string GetInstructions(MedicationDose dose)
        {
            var instructions = new List<string>();

            if (!string.IsNullOrWhiteSpace(dose.Instructions))
                instructions.Add(dose.Instructions);

            if (dose.RequiresFood)
                instructions.Add("Take with food");

            if (dose.RequiresEmptyStomach)
                instructions.Add("Take on empty stomach");

            return instructions.Any() ? string.Join(" • ", instructions) : "No special instructions";
        }

        private Brush GetAccentBrush(TimeSpan scheduledTime)
        {
            // Color code by time of day
            var hour = scheduledTime.Hours;

            if (hour >= 6 && hour < 12)
                return new SolidColorBrush(Color.FromRgb(34, 197, 94));  // Green - Morning

            if (hour >= 12 && hour < 17)
                return new SolidColorBrush(Color.FromRgb(59, 130, 246));  // Blue - Afternoon

            if (hour >= 17 && hour < 21)
                return new SolidColorBrush(Color.FromRgb(251, 146, 60));  // Orange - Evening

            return new SolidColorBrush(Color.FromRgb(139, 92, 246));  // Purple - Night
        }

        #endregion
    }
}
