namespace MedicationManager.Core.Models.Schedule
{
    /// <summary>
    /// Individual medication dose within a schedule entry
    /// </summary>
    public class MedicationDose
    {
        /// <summary>
        /// The user's medication being taken
        /// </summary>
        public UserMedication Medication { get; set; } = null!;

        /// <summary>
        /// Amount to take
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Dose unit
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Specific instructions for this dose
        /// </summary>
        public string Instructions { get; set; } = string.Empty;

        /// <summary>
        /// Whether this medication requires food
        /// </summary>
        public bool RequiresFood { get; set; }

        /// <summary>
        /// Whether this medication requires empty stomach
        /// </summary>
        public bool RequiresEmptyStomach { get; set; }

        /// <summary>
        /// Formatted dose string for display (e.g., "500 mg")
        /// </summary>
        public string FormattedDose => $"{Amount:G29} {Unit}";

        /// <summary>
        /// Display name for the medication
        /// </summary>
        public string DisplayName => Medication?.Medication?.Name ?? "Unknown";
    }
}