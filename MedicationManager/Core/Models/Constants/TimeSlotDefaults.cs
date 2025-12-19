namespace MedicationManager.Core.Models.Constants
{
    /// <summary>
    /// Default time configurations for medication scheduling
    /// </summary>
    public static class TimeSlotDefaults
    {
        /// <summary>
        /// Default time for morning medications
        /// </summary>
        public static readonly TimeSpan MorningTime = new(8, 0, 0); // 8:00 AM

        /// <summary>
        /// Default time for noon medications
        /// </summary>
        public static readonly TimeSpan NoonTime = new(12, 0, 0); // 12:00 PM

        /// <summary>
        /// Default time for evening medications
        /// </summary>
        public static readonly TimeSpan EveningTime = new(18, 0, 0); // 6:00 PM

        /// <summary>
        /// Default time for bedtime medications
        /// </summary>
        public static readonly TimeSpan BedtimeTime = new(22, 0, 0); // 10:00 PM

        /// <summary>
        /// Map of timing preferences to default times
        /// </summary>
        public static readonly Dictionary<TimingPreference, TimeSpan> DefaultTimes = new()
        {
            { TimingPreference.Morning, MorningTime },
            { TimingPreference.Noon, NoonTime },
            { TimingPreference.Evening, EveningTime },
            { TimingPreference.Bedtime, BedtimeTime }
        };

        /// <summary>
        /// Map of timing preferences to time ranges
        /// </summary>
        public static readonly Dictionary<TimingPreference, (TimeSpan Start, TimeSpan End)> TimeRanges = new()
        {
            { TimingPreference.Morning, (new TimeSpan(6, 0, 0), new TimeSpan(11, 0, 0)) },
            { TimingPreference.Noon, (new TimeSpan(11, 0, 0), new TimeSpan(14, 0, 0)) },
            { TimingPreference.Evening, (new TimeSpan(14, 0, 0), new TimeSpan(20, 0, 0)) },
            { TimingPreference.Bedtime, (new TimeSpan(20, 0, 0), new TimeSpan(23, 59, 59)) }
        };
    }
}