namespace MedicationManager.Core.Models.Warnings
{
    /// <summary>
    /// Represents a dosage warning for OTC medications
    /// </summary>
    public class DosageWarning
    {
        /// <summary>
        /// Name of the medication causing the warning
        /// </summary>
        public string MedicationName { get; set; } = string.Empty;

        /// <summary>
        /// Current total daily dose
        /// </summary>
        public decimal CurrentDailyDose { get; set; }

        /// <summary>
        /// Maximum recommended daily dose
        /// </summary>
        public decimal MaxRecommendedDose { get; set; }

        /// <summary>
        /// Dose unit (mg, tablets, etc.)
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Warning message to display to user
        /// </summary>
        public string Warning { get; set; } = string.Empty;

        /// <summary>
        /// Severity level of the warning
        /// </summary>
        public WarningLevel Level { get; set; }

        /// <summary>
        /// Percentage of maximum dose (calculated property)
        /// </summary>
        public decimal PercentageOfMax => MaxRecommendedDose > 0
            ? (CurrentDailyDose / MaxRecommendedDose) * 100
            : 0;
    }
}