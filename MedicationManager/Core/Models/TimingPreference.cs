namespace MedicationManager.Core.Models
{
    /// <summary>
    /// Time preferences for medication scheduling
    /// </summary>
    public enum TimingPreference
    {
        Morning = 1,    // 6 AM - 11 AM
        Noon = 2,       // 11 AM - 2 PM  
        Evening = 3,    // 2 PM - 8 PM
        Bedtime = 4     // 8 PM - 11 PM
    }
}
