using MedicationManager.Core.Models.Warnings;

namespace MedicationManager.Core.Models.Schedule
{
    /// <summary>
    /// Complete daily medication schedule with all warnings
    /// </summary>
    public class DailySchedule
    {
        /// <summary>
        /// When this schedule was generated
        /// </summary>
        public DateTime GeneratedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// All scheduled medication entries grouped by time
        /// </summary>
        public List<ScheduleEntry> Entries { get; set; } = new();

        /// <summary>
        /// Drug interaction warnings
        /// </summary>
        public List<DrugInteraction> Interactions { get; set; } = new();

        /// <summary>
        /// Dosage warnings for individual medications
        /// </summary>
        public List<DosageWarning> DosageWarnings { get; set; } = new();

        /// <summary>
        /// Warnings about duplicate active ingredients
        /// </summary>
        public List<DuplicateActiveIngredientWarning> DuplicationWarnings { get; set; } = new();

        /// <summary>
        /// Food and alcohol interaction warnings
        /// </summary>
        public List<FoodInteraction> FoodInteractions { get; set; } = new();

        /// <summary>
        /// Total number of issues across all warning types
        /// </summary>
        public int TotalIssues => Interactions.Count + DosageWarnings.Count +
                                   DuplicationWarnings.Count + FoodInteractions.Count;

        /// <summary>
        /// Number of high-severity issues
        /// </summary>
        public int HighSeverityIssues =>
            Interactions.Count(i => i.SeverityLevel >= InteractionSeverity.Major) +
            DosageWarnings.Count(w => w.Level >= WarningLevel.Warning) +
            FoodInteractions.Count(f => f.Severity >= InteractionSeverity.Major);

        /// <summary>
        /// Whether this schedule has any warnings
        /// </summary>
        public bool HasWarnings => TotalIssues > 0;

        /// <summary>
        /// Whether this schedule has critical warnings
        /// </summary>
        public bool HasCriticalWarnings => HighSeverityIssues > 0;

        /// <summary>
        /// Total number of medications in the schedule
        /// </summary>
        public int TotalMedications => Entries.SelectMany(e => e.Medications).Select(m => m.Medication.Id).Distinct().Count();
    }
}