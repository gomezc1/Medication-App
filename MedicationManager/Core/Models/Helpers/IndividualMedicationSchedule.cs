namespace MedicationManager.Core.Models.Helpers
{
    /// <summary>
    /// Helper class used during schedule generation before grouping
    /// </summary>
    public class IndividualMedicationSchedule
    {
        /// <summary>
        /// User medication being scheduled
        /// </summary>
        public UserMedication UserMedication { get; set; } = null!;

        /// <summary>
        /// Scheduled time
        /// </summary>
        public TimeSpan ScheduledTime { get; set; }

        /// <summary>
        /// Dose amount for this time
        /// </summary>
        public decimal DoseAmount { get; set; }

        /// <summary>
        /// Dose unit
        /// </summary>
        public string DoseUnit { get; set; } = string.Empty;

        /// <summary>
        /// Instructions for this dose
        /// </summary>
        public string Instructions { get; set; } = string.Empty;

        /// <summary>
        /// Priority for conflict resolution (higher = more important)
        /// </summary>
        public int Priority { get; set; }
    }
}