namespace MedicationScheduler.Services.Interfaces
{
    public interface IDialogService
    {
        /// <summary>
        /// Show an information message
        /// </summary>
        Task ShowInformationAsync(string title, string message);

        /// <summary>
        /// Show a success message
        /// </summary>
        Task ShowSuccessAsync(string title, string message);

        /// <summary>
        /// Show a warning message
        /// </summary>
        Task ShowWarningAsync(string title, string message);

        /// <summary>
        /// Show an error message
        /// </summary>
        Task ShowErrorAsync(string title, string message);

        /// <summary>
        /// Show a confirmation dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        /// <returns>True if user confirmed, false otherwise</returns>
        Task<bool> ShowConfirmationAsync(string title, string message);

        /// <summary>
        /// Show an input dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        /// <param name="defaultValue">Default input value</param>
        /// <returns>User input or null if cancelled</returns>
        Task<string?> ShowInputAsync(string title, string message, string defaultValue = "");

        /// <summary>
        /// Show a selection dialog
        /// </summary>
        /// <typeparam name="T">Type of items to select from</typeparam>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        /// <param name="items">Items to choose from</param>
        /// <returns>Selected item or null if cancelled</returns>
        Task<T?> ShowSelectionAsync<T>(string title, string message, IEnumerable<T> items) where T : class;

        /// <summary>
        /// Show a file open dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="filter">File filter (e.g., "Text files (*.txt)|*.txt")</param>
        /// <returns>Selected file path or null if cancelled</returns>
        Task<string?> ShowOpenFileDialogAsync(string title, string filter = "All files (*.*)|*.*");

        /// <summary>
        /// Show a file save dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="filter">File filter (e.g., "Text files (*.txt)|*.txt")</param>
        /// <param name="defaultFileName">Default file name</param>
        /// <returns>Selected file path or null if cancelled</returns>
        Task<string?> ShowSaveFileDialogAsync(string title, string filter = "All files (*.*)|*.*", string defaultFileName = "");

        /// <summary>
        /// Show a folder browser dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <returns>Selected folder path or null if cancelled</returns>
        Task<string?> ShowFolderBrowserDialogAsync(string title);
    }
}
