using MedicationScheduler.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MedicationScheduler.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();

            // DataContext will be set by NavigationService
            Loaded += DashboardView_Loaded;
        }

        private async void DashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            // Load dashboard when view is displayed
            if (DataContext is DashboardViewModel viewModel)
            {
                await viewModel.LoadDashboardCommand.ExecuteAsync(null);
            }
        }

    }
}
