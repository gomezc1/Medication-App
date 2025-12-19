using System.Globalization;
using System.Windows.Data;

namespace MedicationScheduler.Converters
{
    /// <summary>
    /// Converts boolean IsEditMode to button text
    /// </summary>
    public class EditModeToButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isEditMode)
            {
                return isEditMode ? "Save Changes" : "Add Medication";
            }
            return "Save";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
