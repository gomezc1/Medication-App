namespace MedicationManager.Core.Models
{
    /// <summary>
    /// Severity levels for drug interactions
    /// </summary>
    public enum InteractionSeverity
    {
        None = 0,        // Severity none
        Minor = 1,           // Minor clinical significance
        Moderate = 2,        // Moderate clinical significance
        Major = 3,           // Major clinical significance
        Contraindicated = 4  // Do not use together,
    }
}