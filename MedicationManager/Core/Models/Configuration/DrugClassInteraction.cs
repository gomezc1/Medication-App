namespace MedicationManager.Core.Models.Configuration
{
    /// <summary>
    /// Known interactions between drug classes
    /// </summary>
    public class DrugClassInteraction
    {
        /// <summary>
        /// First drug class
        /// </summary>
        public string DrugClass1 { get; set; } = string.Empty;

        /// <summary>
        /// Second drug class
        /// </summary>
        public string DrugClass2 { get; set; } = string.Empty;

        /// <summary>
        /// Severity of the interaction
        /// </summary>
        public InteractionSeverity Severity { get; set; }

        /// <summary>
        /// Description of the interaction
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Clinical recommendation
        /// </summary>
        public string Recommendation { get; set; } = string.Empty;
    }
}