namespace MedicationManager.Core.Models.Helpers
{
    /// <summary>
    /// Helper class for tracking cumulative dosages of active ingredients
    /// </summary>
    public class CumulativeDosage
    {
        /// <summary>
        /// Active ingredient name
        /// </summary>
        public string Ingredient { get; set; } = string.Empty;

        /// <summary>
        /// Total daily dose across all products
        /// </summary>
        public decimal TotalDailyDose { get; set; }

        /// <summary>
        /// Dose unit
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// List of medication names contributing to this total
        /// </summary>
        public List<string> MedicationNames { get; set; } = new();

        /// <summary>
        /// Number of products containing this ingredient
        /// </summary>
        public int ProductCount => MedicationNames.Count;
    }
}