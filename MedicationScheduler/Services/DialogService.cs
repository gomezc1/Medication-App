using MedicationScheduler.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Windows;

namespace MedicationScheduler.Services
{
    /// <summary>
    /// Service for displaying dialogs and message boxes
    /// </summary>
    public class DialogService : IDialogService
    {
        private readonly ILogger<DialogService> _logger;

        public DialogService(ILogger<DialogService> logger)
        {
            _logger = logger;
        }

        public Task ShowInformationAsync(string title, string message)
        {
            return Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        message,
                        title,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                });
            });
        }

        public Task ShowSuccessAsync(string title, string message)
        {
            return Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        message,
                        title,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                });
            });
        }

        public Task ShowWarningAsync(string title, string message)
        {
            return Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        message,
                        title,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                });
            });
        }

        public Task ShowErrorAsync(string title, string message)
        {
            return Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _logger.LogError("Dialog Error - {Title}: {Message}", title, message);
                    MessageBox.Show(
                        message,
                        title,
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            });
        }

        public Task<bool> ShowConfirmationAsync(string title, string message)
        {
            return Task.Run(() =>
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    var result = MessageBox.Show(
                        message,
                        title,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    return result == MessageBoxResult.Yes;
                });
            });
        }

        public Task<string?> ShowInputAsync(string title, string message, string defaultValue = "")
        {
            return Task.Run(() =>
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    var inputDialog = new InputDialog(title, message, defaultValue);
                    var result = inputDialog.ShowDialog();

                    return result == true ? inputDialog.InputText : null;
                });
            });
        }

        public Task<T?> ShowSelectionAsync<T>(string title, string message, IEnumerable<T> items) where T : class
        {
            return Task.Run(() =>
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    var itemsList = items.ToList();
                    if (!itemsList.Any())
                    {
                        ShowWarningAsync("No Items", "No items available to select from.").Wait();
                        return null;
                    }

                    var selectionDialog = new SelectionDialog<T>(title, message, itemsList);
                    var result = selectionDialog.ShowDialog();

                    return result == true ? selectionDialog.SelectedItem : null;
                });
            });
        }

        public Task<string?> ShowOpenFileDialogAsync(string title, string filter = "All files (*.*)|*.*")
        {
            return Task.Run(() =>
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    var dialog = new OpenFileDialog
                    {
                        Title = title,
                        Filter = filter,
                        CheckFileExists = true,
                        CheckPathExists = true
                    };

                    var result = dialog.ShowDialog();
                    return result == true ? dialog.FileName : null;
                });
            });
        }

        public Task<string?> ShowSaveFileDialogAsync(string title, string filter = "All files (*.*)|*.*", string defaultFileName = "")
        {
            return Task.Run(() =>
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    var dialog = new SaveFileDialog
                    {
                        Title = title,
                        Filter = filter,
                        FileName = defaultFileName,
                        CheckPathExists = true
                    };

                    var result = dialog.ShowDialog();
                    return result == true ? dialog.FileName : null;
                });
            });
        }

        public Task<string?> ShowFolderBrowserDialogAsync(string title)
        {
            return Task.Run(() =>
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    // Using WPF's OpenFileDialog in folder selection mode (workaround)
                    var dialog = new OpenFileDialog
                    {
                        Title = title,
                        ValidateNames = false,
                        CheckFileExists = false,
                        CheckPathExists = true,
                        FileName = "Select Folder"
                    };

                    var result = dialog.ShowDialog();
                    if (result == true)
                    {
                        return System.IO.Path.GetDirectoryName(dialog.FileName);
                    }
                    return null;
                });
            });
        }
    }

    // ============================================================================
    // Helper Dialog Windows
    // ============================================================================

    /// <summary>
    /// Simple input dialog window
    /// </summary>
    internal class InputDialog : Window
    {
        private readonly System.Windows.Controls.TextBox _textBox;

        public string InputText => _textBox.Text;

        public InputDialog(string title, string message, string defaultValue = "")
        {
            Title = title;
            Width = 400;
            Height = 180;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;

            var grid = new System.Windows.Controls.Grid();
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.Margin = new Thickness(10);

            var messageLabel = new System.Windows.Controls.Label
            {
                Content = message,
                Margin = new Thickness(0, 0, 0, 10)
            };
            System.Windows.Controls.Grid.SetRow(messageLabel, 0);
            grid.Children.Add(messageLabel);

            _textBox = new System.Windows.Controls.TextBox
            {
                Text = defaultValue,
                Margin = new Thickness(0, 0, 0, 15)
            };
            System.Windows.Controls.Grid.SetRow(_textBox, 1);
            grid.Children.Add(_textBox);

            var buttonPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            System.Windows.Controls.Grid.SetRow(buttonPanel, 2);

            var okButton = new System.Windows.Controls.Button
            {
                Content = "OK",
                Width = 80,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };
            okButton.Click += (s, e) => { DialogResult = true; Close(); };
            buttonPanel.Children.Add(okButton);

            var cancelButton = new System.Windows.Controls.Button
            {
                Content = "Cancel",
                Width = 80,
                IsCancel = true
            };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };
            buttonPanel.Children.Add(cancelButton);

            grid.Children.Add(buttonPanel);
            Content = grid;

            _textBox.Focus();
            _textBox.SelectAll();
        }
    }

    /// <summary>
    /// Simple selection dialog window
    /// </summary>
    internal class SelectionDialog<T> : Window where T : class
    {
        private readonly System.Windows.Controls.ListBox _listBox;

        public T? SelectedItem => _listBox.SelectedItem as T;

        public SelectionDialog(string title, string message, IEnumerable<T> items)
        {
            Title = title;
            Width = 450;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.CanResize;

            var grid = new System.Windows.Controls.Grid();
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.Margin = new Thickness(10);

            var messageLabel = new System.Windows.Controls.Label
            {
                Content = message,
                Margin = new Thickness(0, 0, 0, 10)
            };
            System.Windows.Controls.Grid.SetRow(messageLabel, 0);
            grid.Children.Add(messageLabel);

            _listBox = new System.Windows.Controls.ListBox
            {
                Margin = new Thickness(0, 0, 0, 15)
            };
            foreach (var item in items)
            {
                _listBox.Items.Add(item);
            }
            _listBox.MouseDoubleClick += (s, e) => { DialogResult = true; Close(); };
            System.Windows.Controls.Grid.SetRow(_listBox, 1);
            grid.Children.Add(_listBox);

            var buttonPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            System.Windows.Controls.Grid.SetRow(buttonPanel, 2);

            var okButton = new System.Windows.Controls.Button
            {
                Content = "OK",
                Width = 80,
                Margin = new Thickness(0, 0, 10, 0),
                IsDefault = true
            };
            okButton.Click += (s, e) => { DialogResult = true; Close(); };
            buttonPanel.Children.Add(okButton);

            var cancelButton = new System.Windows.Controls.Button
            {
                Content = "Cancel",
                Width = 80,
                IsCancel = true
            };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };
            buttonPanel.Children.Add(cancelButton);

            grid.Children.Add(buttonPanel);
            Content = grid;

            if (_listBox.Items.Count > 0)
            {
                _listBox.SelectedIndex = 0;
            }
            _listBox.Focus();
        }
    }
}
