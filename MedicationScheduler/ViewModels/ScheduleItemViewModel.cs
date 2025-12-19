using MedicationManager.Core.Models;
using System.Windows.Media;

namespace MedicationScheduler.ViewModels
{
    /// <summary>
    /// View model for individual schedule items displayed in the UI
    /// </summary>
    public class ScheduleItemViewModel
    {
        public string TimeLabel { get; set; } = string.Empty;
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public Brush AccentBrush { get; set; } = Brushes.Blue;
        public TimeSpan ScheduledTime { get; set; }
        public TimingPreference TimeSlot { get; set; }
        public UserMedication? UserMedication { get; set; }
    }
}
