using MedicationScheduler.Services.Interfaces;
using System.Windows;

namespace MedicationScheduler
{
    public partial class MainWindow : Window
    {
        private readonly INavigationService _navigationService;

        public MainWindow(INavigationService navigationService)
        {
            InitializeComponent();
            _navigationService = navigationService;

            // Navigate to dashboard on startup
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Show dashboard by default
            _navigationService.NavigateToDashboard();
            _navigationService.PropertyChanged += _navigationService_PropertyChanged;
        }

        private void _navigationService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_navigationService.HasWarnings)
            {
                WarningsNav.Foreground = System.Windows.Media.Brushes.Orange;
            } else
            {
                WarningsNav.Foreground = System.Windows.Media.Brushes.White;
            }
        }

        // Navigation event handlers
        private void DashboardNav_Checked(object sender, RoutedEventArgs e)
        {
            if (_navigationService != null)
            {
                _navigationService.NavigateToDashboard();
            }
        }

        private void MedicationListNav_Checked(object sender, RoutedEventArgs e)
        {
            if (_navigationService != null)
            {
                _navigationService.NavigateToMedicationList();
            }
        }

        private void DailyScheduleNav_Checked(object sender, RoutedEventArgs e)
        {
            if (_navigationService != null)
            {
                _navigationService.NavigateToSchedule();
            }
        }

        private void WarningsNav_Checked(object sender, RoutedEventArgs e)
        {
            if (_navigationService != null)
            {
                _navigationService.NavigateToInteractionWarnings();
            }
        }
    }
}
