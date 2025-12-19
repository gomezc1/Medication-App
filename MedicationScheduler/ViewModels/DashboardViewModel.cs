using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MedicationManager.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace MedicationScheduler.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IMedicationService _medicationService;
        private readonly IScheduleService _scheduleService;
        private readonly IInteractionService _interactionService;
        private readonly ILogger<DashboardViewModel> _logger;

        #region Observable Properties

        [ObservableProperty]
        private int _activeMedicationsCount;

        [ObservableProperty]
        private int _totalMedicationsCount;

        [ObservableProperty]
        private string _upcomingDose = "No doses";

        [ObservableProperty]
        private int _interactionsCount;

        [ObservableProperty]
        private ObservableCollection<DashboardScheduleItem> _todaySchedule = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _hasSchedule;

        #endregion

        public DashboardViewModel(
            IMedicationService medicationService,
            IScheduleService scheduleService,
            IInteractionService interactionService,
            ILogger<DashboardViewModel> logger)
        {
            _medicationService = medicationService;
            _scheduleService = scheduleService;
            _interactionService = interactionService;
            _logger = logger;
        }

        #region Commands

        [RelayCommand]
        public async Task LoadDashboardAsync()
        {
            try
            {
                IsLoading = true;

                // Get all medications
                var allMedications = await _medicationService.GetAllUserMedicationsAsync();
                var activeMedications = allMedications.Where(m => m.IsActive).ToList();

                // Update medication counts
                TotalMedicationsCount = allMedications.Count();
                ActiveMedicationsCount = activeMedications.Count;

                if (!activeMedications.Any())
                {
                    UpcomingDose = "No medications";
                    InteractionsCount = 0;
                    TodaySchedule.Clear();
                    HasSchedule = false;
                    return;
                }

                // Generate schedule
                var schedule = await _scheduleService.GenerateScheduleAsync(activeMedications);

                // Get upcoming dose
                var now = DateTime.Now.TimeOfDay;
                var upcomingEntry = schedule.Entries
                    .Where(e => e.Time > now)
                    .OrderBy(e => e.Time)
                    .FirstOrDefault();

                if (upcomingEntry != null)
                {
                    UpcomingDose = upcomingEntry.FormattedTime;
                }
                else
                {
                    // No more doses today, show next morning dose
                    var firstMorningDose = schedule.Entries
                        .OrderBy(e => e.Time)
                        .FirstOrDefault();

                    UpcomingDose = firstMorningDose != null
                        ? $"Tomorrow {firstMorningDose.FormattedTime}"
                        : "No schedule";
                }

                // Check interactions
                var interactionResult = await _interactionService.CheckInteractionsAsync(activeMedications);
                InteractionsCount = interactionResult.DrugInteractions.Count +
                                  interactionResult.DuplicateWarnings.Count;

                // Build today's schedule (show next 6 doses)
                TodaySchedule.Clear();
                var upcomingEntries = schedule.Entries
                    .OrderBy(e => e.Time)
                    .Where(e => e.Time >= now)
                    .Take(6)
                    .ToList();

                // If less than 6 upcoming, add some from beginning of day
                if (upcomingEntries.Count < 6)
                {
                    var earlierEntries = schedule.Entries
                        .OrderBy(e => e.Time)
                        .Where(e => e.Time < now)
                        .Take(6 - upcomingEntries.Count)
                        .ToList();

                    upcomingEntries.AddRange(earlierEntries);
                }

                foreach (var entry in upcomingEntries.OrderBy(e => e.Time))
                {
                    foreach (var dose in entry.Medications)
                    {
                        TodaySchedule.Add(new DashboardScheduleItem
                        {
                            TimeLabel = entry.FormattedTime,
                            Name = dose.Medication.Medication.Name,
                            Dosage = $"{dose.Amount} {dose.Unit}",
                            AccentBrush = GetAccentBrush(entry.Time),
                            Time = entry.Time
                        });
                    }
                }

                HasSchedule = TodaySchedule.Any();

                _logger.LogInformation("Dashboard loaded: {Active} active meds, {Interactions} interactions",
                    ActiveMedicationsCount, InteractionsCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                // Don't show error dialog on dashboard - just log it
                UpcomingDose = "Error loading";
                InteractionsCount = 0;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshDashboardAsync()
        {
            await LoadDashboardAsync();
        }

        #endregion

        #region Helper Methods

        private Brush GetAccentBrush(TimeSpan scheduledTime)
        {
            // Color code by time of day
            var hour = scheduledTime.Hours;

            if (hour >= 6 && hour < 12)
                return new SolidColorBrush(Color.FromRgb(30, 136, 229));  // Blue - Morning

            if (hour >= 12 && hour < 17)
                return new SolidColorBrush(Color.FromRgb(23, 184, 166));  // Teal - Afternoon

            if (hour >= 17 && hour < 21)
                return new SolidColorBrush(Color.FromRgb(245, 124, 32));  // Orange - Evening

            return new SolidColorBrush(Color.FromRgb(139, 92, 246));  // Purple - Night
        }

        #endregion
    }

    /// <summary>
    /// View model for dashboard schedule items
    /// </summary>
    public class DashboardScheduleItem
    {
        public string TimeLabel { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public Brush AccentBrush { get; set; } = Brushes.Blue;
        public TimeSpan Time { get; set; }
    }
}