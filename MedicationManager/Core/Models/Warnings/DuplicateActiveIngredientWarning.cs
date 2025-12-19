namespace MedicationManager.Core.Models.Warnings
{
    /// <summary>
    /// Warning for duplicate active ingredients across multiple products
    /// </summary>
    public class DuplicateActiveIngredientWarning
    {
        /// <summary>
        /// Name of the duplicated active ingredient
        /// </summary>
        public string ActiveIngredient { get; set; } = string.Empty;

        /// <summary>
        /// List of medication names containing this ingredient
        /// </summary>
        public List<string> MedicationNames { get; set; } = new();

        /// <summary>
        /// Warning message about the duplication
        /// </summary>
        public string Warning { get; set; } = string.Empty;

        /// <summary>
        /// Total daily dose across all products
        /// </summary>
        public decimal TotalDailyDose { get; set; }

        /// <summary>
        /// Dose unit
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// Number of products containing this ingredient
        /// </summary>
        public int ProductCount => MedicationNames.Count;
    }
}