namespace MedicationManager.Core.Models.Warnings
{
    /// <summary>
    /// Warning levels for various types of alerts
    /// </summary>
    public enum WarningLevel
    {
        Info = 1,      // Informational message
        Caution = 2,   // Use caution
        Warning = 3,   // Important warning
        Danger = 4     // Dangerous situation
    }
}