using MedicationManager.Core.Models;
using MedicationScheduler.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace MedicationScheduler.Views
{
    /// <summary>
    /// Interaction logic for AddEditMedicationView.xaml
    /// </summary>
    public partial class AddEditMedicationView : UserControl
    {
        public AddEditMedicationView()
        {
            InitializeComponent();

            // DataContext is set by the NavigationService or dialog host
            // No need to set it here - it will be injected

            Loaded += AddEditMedicationView_Loaded;
        }

        private void AddEditMedicationView_Loaded(object sender, RoutedEventArgs e)
        {
            // Optional: You can do initialization here if needed
            // The ViewModel is already set by NavigationService at this point

            if (DataContext is AddEditMedicationViewModel viewModel)
            {
                // Sync timing preference button states if editing
                SyncTimingPreferenceButtons();
            }
        }

        private void TimingPref_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton btn &&
                btn.Tag is string prefString &&
                DataContext is AddEditMedicationViewModel vm)
            {
                if (Enum.TryParse<TimingPreference>(prefString, out var pref))
                {
                    vm.AddTimingPreferenceCommand.Execute(pref);
                }
            }
        }

        private void TimingPref_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton btn &&
                btn.Tag is string prefString &&
                DataContext is AddEditMedicationViewModel vm)
            {
                if (Enum.TryParse<TimingPreference>(prefString, out var pref))
                {
                    vm.RemoveTimingPreferenceCommand.Execute(pref);
                }
            }
        }

        private void SyncTimingPreferenceButtons()
        {
            if (DataContext is AddEditMedicationViewModel vm)
            {
                MorningBtn.IsChecked = vm.SelectedTimingPreferences.Contains(TimingPreference.Morning);
                NoonBtn.IsChecked = vm.SelectedTimingPreferences.Contains(TimingPreference.Noon);
                EveningBtn.IsChecked = vm.SelectedTimingPreferences.Contains(TimingPreference.Evening);
                BedtimeBtn.IsChecked = vm.SelectedTimingPreferences.Contains(TimingPreference.Bedtime);
            }
        }
    }
}
