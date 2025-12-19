using MedicationScheduler.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MedicationScheduler.Views
{
    public partial class WarningsView : UserControl
    {
        public WarningsView()
        {
            InitializeComponent();

            // DataContext will be set by NavigationService
            Loaded += WarningsView_Loaded;
        }

        private async void WarningsView_Loaded(object sender, RoutedEventArgs e)
        {
            // Load warnings when view is displayed
            if (DataContext is InteractionWarningsViewModel viewModel)
            {
                await viewModel.LoadWarningsCommand.ExecuteAsync(null);
            }
        }
    }
}
