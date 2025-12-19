// ============================================================================
// File: MedicationScheduler.Views/MedicationListView.xaml.cs
// Description: Code-behind for MedicationListView
// ============================================================================

using MedicationManager.Core.Models;
using MedicationScheduler.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MedicationScheduler.Views
{
    /// <summary>
    /// Interaction logic for MedicationListView.xaml
    /// </summary>
    public partial class MedicationListView : UserControl
    {
        public MedicationListView()
        {
            InitializeComponent();

            // Load medications when the view is loaded
            Loaded += MedicationListView_Loaded;
        }

        private async void MedicationListView_Loaded(object sender, RoutedEventArgs e)
        {
            // Automatically load medications when view is displayed
            if (DataContext is MedicationListViewModel viewModel)
            {
                await viewModel.LoadMedicationsCommand.ExecuteAsync(null);
            }
        }

        private void CardMenuButton_Click(object sender, RoutedEventArgs e)
        {
            // Open the context menu when the button is clicked
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }

        private async void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.CommandParameter is UserMedication medication &&
                DataContext is MedicationListViewModel viewModel)
            {
                await viewModel.EditMedicationCommand.ExecuteAsync(medication);
            }
        }

        private async void ToggleActiveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.CommandParameter is UserMedication medication &&
                DataContext is MedicationListViewModel viewModel)
            {
                await viewModel.ToggleActiveStatusCommand.ExecuteAsync(medication);
            }
        }

        private async void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem &&
                menuItem.CommandParameter is UserMedication medication &&
                DataContext is MedicationListViewModel viewModel)
            {
                await viewModel.DeleteMedicationCommand.ExecuteAsync(medication);
            }
        }
    }
}
