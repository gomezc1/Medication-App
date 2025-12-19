namespace MedicationManager.Core.Models.Configuration
{
    /// <summary>
    /// Maximum dosage limits for OTC medications
    /// </summary>
    public class OtcDosageLimit
    {
        /// <summary>
        /// Display name of the active ingredient
        /// </summary>
        public string IngredientName { get; set; } = string.Empty;

        /// <summary>
        /// Maximum safe daily dose
        /// </summary>
        public decimal MaxDailyDose { get; set; }

        /// <summary>
        /// Unit for the maximum dose
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Maximum duration of use in days (for OTC products)
        /// </summary>
        public int MaxDurationDays { get; set; }

        /// <summary>
        /// Warning message when limit is exceeded
        /// </summary>
        public string WarningMessage { get; set; } = string.Empty;

        /// <summary>
        /// Additional safety information
        /// </summary>
        public string SafetyNotes { get; set; } = string.Empty;
    }
}