using MedicationScheduler.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MedicationScheduler.Views
{
    /// <summary>
    /// Interaction logic for DailyScheduleView.xaml
    /// </summary>
    public partial class DailyScheduleView : UserControl
    {
        public DailyScheduleView()
        {
            InitializeComponent();

            // DataContext will be set by NavigationService
            Loaded += DailyScheduleView_Loaded;
        }

        private async void DailyScheduleView_Loaded(object sender, RoutedEventArgs e)
        {
            // Load schedule when view is displayed
            if (DataContext is ScheduleViewModel viewModel)
            {
                await viewModel.LoadScheduleCommand.ExecuteAsync(null);
            }
        }
    }
}