namespace MedicationManager.Core.Models.Schedule
{
    /// <summary>
    /// Medications scheduled for a specific time window
    /// </summary>
    public class ScheduleEntry
    {
        /// <summary>
        /// Scheduled time
        /// </summary>
        public TimeSpan Time { get; set; }

        /// <summary>
        /// Time slot category
        /// </summary>
        public TimingPreference TimeSlot { get; set; }

        /// <summary>
        /// All medications scheduled for this time
        /// </summary>
        public List<MedicationDose> Medications { get; set; } = new();

        /// <summary>
        /// General instructions that apply to all medications at this time
        /// </summary>
        public string GeneralInstructions { get; set; } = string.Empty;

        /// <summary>
        /// Formatted time string for display (e.g., "8:00 AM")
        /// </summary>
        public string FormattedTime => DateTime.Today.Add(Time).ToString("h:mm tt");

        /// <summary>
        /// Whether any medications at this time require food
        /// </summary>
        public bool RequiresFood => Medications.Any(m => m.RequiresFood);

        /// <summary>
        /// Whether any medications at this time require empty stomach
        /// </summary>
        public bool RequiresEmptyStomach => Medications.Any(m => m.RequiresEmptyStomach);
    }
}