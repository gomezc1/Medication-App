using MedicationManager.Core.Models;
using System.ComponentModel;

namespace MedicationScheduler.Services.Interfaces
{
    /// <summary>
    /// Service for handling navigation between views
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Potential property changed event for navigation state
        /// 
        event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raise when warnings are present
        /// 
        bool HasWarnings { get; }

        /// <summary>
        /// Navigate to the dashboard view
        /// </summary>
        void NavigateToDashboard();

        /// <summary>
        /// Navigate to the main view
        /// </summary>
        void NavigateToMain();

        /// <summary>
        /// Navigate to the medication list view
        /// </summary>
        void NavigateToMedicationList();

        /// <summary>
        /// Navigate to the add/edit medication view
        /// </summary>
        /// <param name="medication">Optional medication to edit</param>
        /// <returns>The created or updated medication, or null if cancelled</returns>
        Task<UserMedication?> NavigateToAddEditMedicationAsync(UserMedication? medication = null);

        /// <summary>
        /// Navigate to the schedule view
        /// </summary>
        void NavigateToSchedule();

        /// <summary>
        /// Navigate to the interaction warnings view
        /// </summary>
        void NavigateToInteractionWarnings();

        /// <summary>
        /// Navigate to the settings view
        /// </summary>
        void NavigateToSettings();

        /// <summary>
        /// Navigate to the about view
        /// </summary>
        void NavigateToAbout();

        /// <summary>
        /// Go back to the previous view
        /// </summary>
        void GoBack();

        /// <summary>
        /// Check if navigation back is possible
        /// </summary>
        bool CanGoBack { get; }
    }
}