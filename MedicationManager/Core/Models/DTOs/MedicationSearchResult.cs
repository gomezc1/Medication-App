namespace MedicationManager.Core.Models.DTOs
{
    /// <summary>
    /// Wrapper for medication search results with metadata
    /// </summary>
    public class MedicationSearchResult
    {
        /// <summary>
        /// The medication found
        /// </summary>
        public Medication Medication { get; set; } = null!;

        /// <summary>
        /// Source of the result (Local Database, RxNorm API, OpenFDA API)
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Relevance score for ranking results
        /// </summary>
        public int Relevance { get; set; }

        /// <summary>
        /// Whether this medication is already in user's list
        /// </summary>
        public bool IsInUserList { get; set; }

        /// <summary>
        /// Display text for search result
        /// </summary>
        public string DisplayText => string.IsNullOrEmpty(Medication.GenericName)
            ? Medication.Name
            : $"{Medication.Name} ({Medication.GenericName})";
    }
}