using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace MedicationScheduler.Converters
{
    /// <summary>
    /// Converts count > 0 to true, otherwise false
    /// </summary>
    public class CountToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 0;
            }

            if (value is ICollection collection)
            {
                return collection.Count > 0;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
