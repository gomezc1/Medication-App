using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MedicationScheduler.Converters
{
    /// <summary>
    /// Inverse of CountToVisibilityConverter (shows when count is 0)
    /// </summary>
    public class InverseCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value is ICollection collection)
            {
                return collection.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value is Boolean boolValue)
            {
                return boolValue == true ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
