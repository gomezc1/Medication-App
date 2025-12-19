using System.Globalization;
using System.Windows.Data;

namespace MedicationScheduler.Converters
{
    /// <summary>
    /// Multi-value converter for TimeSpan to formatted string
    /// </summary>
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                var dateTime = DateTime.Today.Add(timeSpan);
                return dateTime.ToString("hh:mm tt");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && TimeSpan.TryParse(str, out var timeSpan))
            {
                return timeSpan;
            }

            // Try parsing as time format (e.g., "08:00 AM")
            if (value is string timeStr && DateTime.TryParse(timeStr, out var dateTime))
            {
                return dateTime.TimeOfDay;
            }

            return TimeSpan.Zero;
        }
    }
}
