using MedicationManager.Core.Models.Warnings;

namespace MedicationManager.Core.Models.DTOs
{
    /// <summary>
    /// Complete results from interaction checking
    /// </summary>
    public class InteractionCheckResult
    {
        /// <summary>
        /// When the check was performed
        /// </summary>
        public DateTime CheckedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Names of medications that were checked
        /// </summary>
        public List<string> CheckedMedications { get; set; } = new();

        /// <summary>
        /// Drug-to-drug interactions found
        /// </summary>
        public List<DrugInteraction> DrugInteractions { get; set; } = new();

        /// <summary>
        /// Duplicate active ingredient warnings
        /// </summary>
        public List<DuplicateActiveIngredientWarning> DuplicateWarnings { get; set; } = new();

        /// <summary>
        /// Food and alcohol interactions
        /// </summary>
        public List<FoodInteraction> FoodInteractions { get; set; } = new();

        /// <summary>
        /// Dosage validation warnings (ADDED)
        /// </summary>
        public List<DosageWarning> DosageWarnings { get; set; } = new();

        /// <summary>
        /// Total number of issues found
        /// </summary>
        public int TotalIssues =>
            DrugInteractions.Count +
            DuplicateWarnings.Count +
            FoodInteractions.Count +
            DosageWarnings.Count;

        /// <summary>
        /// Number of high-severity issues
        /// </summary>
        public int HighSeverityIssues =>
            DrugInteractions.Count(di => di.SeverityLevel >= InteractionSeverity.Major) +
            FoodInteractions.Count(fi => fi.Severity >= InteractionSeverity.Major) +
            DosageWarnings.Count(dw => dw.Level >= WarningLevel.Warning);

        /// <summary>
        /// Whether any issues were found
        /// </summary>
        public bool HasIssues => TotalIssues > 0;

        /// <summary>
        /// Whether any critical issues were found
        /// </summary>
        public bool HasCriticalIssues => HighSeverityIssues > 0;
    }
}