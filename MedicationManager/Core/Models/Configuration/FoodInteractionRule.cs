namespace MedicationManager.Core.Models.Configuration
{
    /// <summary>
    /// Rules for known food and drug interactions
    /// </summary>
    public class FoodInteractionRule
    {
        /// <summary>
        /// Food or beverage item
        /// </summary>
        public string FoodItem { get; set; } = string.Empty;

        /// <summary>
        /// RxCuis that interact with this food
        /// </summary>
        public string[] RxCuis { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Drug names that interact with this food
        /// </summary>
        public string[] DrugNames { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Generic names that interact with this food
        /// </summary>
        public string[] GenericNames { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Type of interaction
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