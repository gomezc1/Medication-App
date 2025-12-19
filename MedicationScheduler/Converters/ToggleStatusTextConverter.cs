using System.Globalization;
using System.Windows.Data;

namespace MedicationScheduler.Converters
{
    /// <summary>
    /// Converts IsActive boolean to toggle menu item text
    /// </summary>
    public class ToggleStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                return isActive ? "Deactivate" : "Activate";
            }
            return "Toggle Status";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
