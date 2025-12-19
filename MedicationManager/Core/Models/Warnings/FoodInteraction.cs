namespace MedicationManager.Core.Models.Warnings
{
    /// <summary>
    /// Represents interactions between medications and food/alcohol
    /// </summary>
    public class FoodInteraction
    {
        /// <summary>
        /// Name of the medication
        /// </summary>
        public string MedicationName { get; set; } = string.Empty;

        /// <summary>
        /// Food or beverage that interacts (e.g., "Alcohol", "Grapefruit", "Dairy")
        /// </summary>
        public string FoodItem { get; set; } = string.Empty;

        /// <summary>
        /// Type of interaction (e.g., "Enhanced Toxicity", "Reduced Absorption")
        /// </summary>
        public string InteractionType { get; set; } = string.Empty;

        /// <summary>
        /// Description of the interaction
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Clinical recommendation
        /// </summary>
        public string Recommendation { get; set; } = string.Empty;

        /// <summary>
        /// Severity of the interaction
        /// </summary>
        public InteractionSeverity Severity { get; set; }
    }
}