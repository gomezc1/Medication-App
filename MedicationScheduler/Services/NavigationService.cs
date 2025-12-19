// ============================================================================
// File: MedicationManager.WPF/Services/NavigationService.cs
// Description: Implementation of navigation service
// ============================================================================

using MedicationManager.Core.Models;
using MedicationScheduler.Services.Interfaces;
using MedicationScheduler.ViewModels;
using MedicationScheduler.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Windows;

namespace MedicationScheduler.Services
{
    /// <summary>
    /// Service for handling navigation between views
    /// </summary>
    public class NavigationService : INavigationService, INotifyPropertyChanged
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NavigationService> _logger;
        private readonly InteractionWarningsViewModel _interactionWarningsViewModel;
        private MainWindow? _mainWindow;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool CanGoBack => false; // Not using stack navigation in this pattern

        public NavigationService(
            IServiceProvider serviceProvider,
            ILogger<NavigationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _interactionWarningsViewModel = _serviceProvider.GetRequiredService<InteractionWarningsViewModel>();
            _interactionWarningsViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(_interactionWarningsViewModel.TotalIssues))
                {
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("HasWarnings"));
                    }
                }
            };
            _interactionWarningsViewModel.LoadWarningsAsync();
        }

        public bool HasWarnings => _interactionWarningsViewModel.TotalIssues > 0;

        private MainWindow GetMainWindow()
        {
            if (_mainWindow == null)
            {
                _mainWindow = Application.Current.Windows
                    .OfType<MainWindow>()
                    .FirstOrDefault();

                if (_mainWindow == null)
                {
                    throw new InvalidOperationException("MainWindow not found. Ensure it's created before navigation.");
                }
            }

            return _mainWindow;
        }

        public void NavigateToMain()
        {
            try
            {
                var mainWindow = GetMainWindow();
                mainWindow.Show();
                mainWindow.Activate();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to main window");
                throw;
            }
        }

        public void NavigateToMedicationList()
        {
            try
            {
                var mainWindow = GetMainWindow();

                // Hide all panels
                HideAllPanels(mainWindow);

                // Show medication list panel
                if (mainWindow.MedicationListPanel != null)
                {
                    mainWindow.MedicationListPanel.Visibility = Visibility.Visible;

                    // Set up ViewModel if not already set
                    if (mainWindow.MedicationListPanel.DataContext == null)
                    {
                        var viewModel = _serviceProvider.GetRequiredService<MedicationListViewModel>();
                        mainWindow.MedicationListPanel.DataContext = viewModel;
                    }

                    // Load medications
                    if (mainWindow.MedicationListPanel.DataContext is MedicationListViewModel vm)
                    {
                        _ = vm.LoadMedicationsCommand.ExecuteAsync(null);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to medication list");
                throw;
            }
        }

        public async Task<UserMedication?> NavigateToAddEditMedicationAsync(UserMedication? medication = null)
        {
            try
            {
                // Create a modal dialog for Add/Edit
                var dialog = new Window
                {
                    Title = medication == null ? "Add Medication" : "Edit Medication",
                    Width = 920,
                    Height = 680,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = GetMainWindow(),
                    ResizeMode = ResizeMode.CanResize,
                    WindowStyle = WindowStyle.SingleBorderWindow,
                    Background = System.Windows.Media.Brushes.White
                };

                // Create new instances for the dialog
                var view = new AddEditMedicationView();
                var viewModel = _serviceProvider.GetRequiredService<AddEditMedicationViewModel>();

                view.DataContext = viewModel;
                dialog.Content = view;

                // Load medication for editing if provided
                if (medication != null)
                {
                    await viewModel.LoadMedicationForEditAsync(medication);
                }

                // Subscribe to Result changes to close dialog
                viewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(viewModel.Result))
                    {
                        if (viewModel.Result != null)
                        {
                            dialog.DialogResult = true; // Saved successfully
                        }
                        else
                        {
                            dialog.DialogResult = false; // Cancelled
                        }
                    }
                };

                // Show dialog and get result
                var result = dialog.ShowDialog();

                return result == true ? viewModel.Result : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to add/edit medication");
                throw;
            }
        }
        public void NavigateToSchedule()
        {
            try
            {
                var mainWindow = GetMainWindow();

                // Hide all panels
                HideAllPanels(mainWindow);

                // Show daily schedule panel
                if (mainWindow.DailySchedulePanel != null)
                {
                    mainWindow.DailySchedulePanel.Visibility = Visibility.Visible;

                    // Set up ViewModel if not already set
                    if (mainWindow.DailySchedulePanel.DataContext == null)
                    {
                        var viewModel = _serviceProvider.GetRequiredService<ScheduleViewModel>();
                        mainWindow.DailySchedulePanel.DataContext = viewModel;
                    }

                    // Load schedule
                    if (mainWindow.DailySchedulePanel.DataContext is ScheduleViewModel vm)
                    {
                        _ = vm.LoadScheduleCommand.ExecuteAsync(null);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to schedule");
                throw;
            }
        }

        public void NavigateToInteractionWarnings()
        {
            try
            {
                var mainWindow = GetMainWindow();

                // Hide all panels
                HideAllPanels(mainWindow);

                // Show warnings panel
                if (mainWindow.WarningsPanel != null)
                {
                    mainWindow.WarningsPanel.Visibility = Visibility.Visible;

                    // Set up ViewModel if not already set
                    if (mainWindow.WarningsPanel.DataContext == null)
                    {
                        var viewModel = _serviceProvider.GetRequiredService<InteractionWarningsViewModel>();
                        mainWindow.WarningsPanel.DataContext = viewModel;
                    }

                    // Load warnings
                    if (mainWindow.WarningsPanel.DataContext is InteractionWarningsViewModel vm)
                    {
                        _ = vm.LoadWarningsCommand.ExecuteAsync(null);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to interaction warnings");
                throw;
            }
        }

        public void NavigateToSettings()
        {
            try
            {
                // Settings can be a separate dialog if needed
                _logger.LogInformation("Settings navigation not yet implemented");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to settings");
                throw;
            }
        }

        public void NavigateToAbout()
        {
            try
            {
                // About can be a separate dialog if needed
                _logger.LogInformation("About navigation not yet implemented");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to about");
                throw;
            }
        }

        public void NavigateToDashboard()
        {
            try
            {
                var mainWindow = GetMainWindow();

                // Hide all panels
                HideAllPanels(mainWindow);

                // Show dashboard panel
                if (mainWindow.DashboardPanel != null)
                {
                    mainWindow.DashboardPanel.Visibility = Visibility.Visible;

                    // Set up ViewModel if not already set
                    if (mainWindow.DashboardPanel.DataContext == null)
                    {
                        var viewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
                        mainWindow.DashboardPanel.DataContext = viewModel;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error navigating to dashboard");
                throw;
            }
        }

        public void GoBack()
        {
            // Not implemented for this navigation pattern
            _logger.LogWarning("GoBack not supported in visibility-based navigation");
        }

        private void HideAllPanels(MainWindow mainWindow)
        {
            // Hide all view panels
            if (mainWindow.DashboardPanel != null)
                mainWindow.DashboardPanel.Visibility = Visibility.Collapsed;

            if (mainWindow.MedicationListPanel != null)
                mainWindow.MedicationListPanel.Visibility = Visibility.Collapsed;

            if (mainWindow.AddEditMedicationPanel != null)
                mainWindow.AddEditMedicationPanel.Visibility = Visibility.Collapsed;

            if (mainWindow.DailySchedulePanel != null)
                mainWindow.DailySchedulePanel.Visibility = Visibility.Collapsed;

            if (mainWindow.WarningsPanel != null)
                mainWindow.WarningsPanel.Visibility = Visibility.Collapsed;

            if (mainWindow.UserProfilePanel != null)
                mainWindow.UserProfilePanel.Visibility = Visibility.Collapsed;
        }
    }
}