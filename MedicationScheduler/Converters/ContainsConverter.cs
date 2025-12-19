using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace MedicationScheduler.Converters
{
    /// <summary>
    /// Checks if a collection contains a specific value
    /// </summary>
    public class ContainsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable enumerable && parameter != null)
            {
                var paramString = parameter.ToString();

                foreach (var item in enumerable)
                {
                    if (item != null && item.ToString() == paramString)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
