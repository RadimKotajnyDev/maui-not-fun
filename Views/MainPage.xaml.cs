using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Storage;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using MauiMacApp.Models;

namespace MauiMacApp.Views
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<MusicItem> MyItems { get; } = new();
        private MusicItem? _selectedItem;

        public MainPage()
        {
            InitializeComponent();
            // Populate 3x3 data (Name, Author, Genre)
            MyItems.Add(new MusicItem { Name = "Bohemian Rhapsody", Author = "Queen", Genre = "Rock" });
            MyItems.Add(new MusicItem { Name = "Imagine", Author = "John Lennon", Genre = "Pop" });
            MyItems.Add(new MusicItem { Name = "Billie Jean", Author = "Michael Jackson", Genre = "Pop" });

            BindingContext = this;

            this.Loaded += (s, e) => CenterMainWindowOnCreation();
        }
        
        private void CenterMainWindowOnCreation()
        {
            try
            {
                var mainWindow = this.Window;
                
                if (mainWindow == null)
                    return;

                var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
                double screenWidth = displayInfo.Width / displayInfo.Density;
                double screenHeight = displayInfo.Height / displayInfo.Density;

                mainWindow.Created += (s, ev) =>
                {
                    var win = (Window)s!;

                    double winWidth = win.Width;
                    double winHeight = win.Height;

                    win.X = (screenWidth - winWidth) / 2;
                    win.Y = (screenHeight - winHeight) / 2;
                };

                if (mainWindow.Handler != null)
                {
                    mainWindow.X = (screenWidth - mainWindow.Width) / 2;
                    mainWindow.Y = (screenHeight - mainWindow.Height) / 2;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error centering main window: {ex.Message}");
            }
        }
        
        private async void OnLoadClicked(object sender, EventArgs e)
        {
            try
            {
                StatusLabel.Text = "Opening file picker...";

                var options = new PickOptions
                {
                    PickerTitle = "Select a file"
                };

                // On Mac Catalyst, MAUI does not support a folder picker. Use FilePicker instead.
                var result = await FilePicker.Default.PickAsync(options);

                if (result != null)
                {
                    var folderPath = Path.GetDirectoryName(result.FullPath);

                    StatusLabel.Text = "File selected";
                    ResultLabel.Text = $"Selected file: {result.FileName}\nFolder: {folderPath}";

                    if (!string.IsNullOrEmpty(folderPath))
                    {
                        // Access to the selected file (and its parent folder) is granted via user-selected entitlement.
                        var files = Directory.GetFiles(folderPath);
                        ResultLabel.Text += $"\nFiles in folder: {files.Length}";
                    }
                }
                else
                {
                    StatusLabel.Text = "No file selected";
                    ResultLabel.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = "Error";
                ResultLabel.Text = $"Error: {ex.Message}";
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            try
            {
                StatusLabel.Text = "Saving file...";

                string fileName = "document.txt";
                string content = "Hello from MAUI macOS App!\nCreated on: " + DateTime.Now.ToString();

                // Use CommunityToolkit.Maui Storage to show a native Save dialog (NSSavePanel on macOS)
                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
                var saveResult = await FileSaver.Default.SaveAsync(fileName, stream, CancellationToken.None);

                if (saveResult.IsSuccessful)
                {
                    StatusLabel.Text = "File saved!";
                    ResultLabel.Text = $"File saved to:\n{saveResult.FilePath}";
                }
                else
                {
                    StatusLabel.Text = "Save canceled";
                    ResultLabel.Text = saveResult.Exception is null
                        ? "User canceled the save dialog."
                        : $"Error: {saveResult.Exception.Message}";
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = "Error";
                ResultLabel.Text = $"Error: {ex.Message}";
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (_selectedItem is null) return;

            // --- FIX 1: "Block" the main UI ---
            // Disable the main layout so the user cannot click anything else
            // Assuming your root layout in MainPage.xaml is named "MainLayout" or you disable the specific button
            this.Content.IsEnabled = false; 

            try
            {
                var editPage = new EditPage(_selectedItem);

                // Define the size of the new window
                double newWidth = 200;
                double newHeight = 200;

                // --- FIX 2: Center the new window ---
                // Get the current main window's position and size
                var mainWin = this.Window;
                double centerX = mainWin.X + (mainWin.Width - newWidth) / 2;
                double centerY = mainWin.Y + (mainWin.Height - newHeight) / 2;

                var newWindow = new Window(editPage)
                {
                    Title = "Edit Music Item",
                    Width = newWidth,
                    Height = newHeight,
                    X = centerX,
                    Y = centerY
                };

                // Force position after the OS creates the window wrapper
                newWindow.Created += (s, e) =>
                {
                    var win = (Window)s!;
                    win.X = centerX;
                    win.Y = centerY;
                };

                Application.Current?.OpenWindow(newWindow);

                // Wait for result (the main UI remains disabled/blocked here)
                var edited = await editPage.CompletionTask;

                if (edited is null)
                {
                    StatusLabel.Text = "Edit canceled";
                }
                else
                {
                    // Update Data
                    var index = MyItems.IndexOf(_selectedItem);
                    if (index >= 0)
                    {
                        MyItems[index] = edited;
                        _selectedItem = edited;
                        StatusLabel.Text = "Row updated";
                    }
                }
            }
            finally
            {
                // --- UNBLOCK ---
                // Re-enable the main UI regardless of save or cancel
                this.Content.IsEnabled = true;
            }
        }
        
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedItem = e.CurrentSelection?.Count > 0 ? e.CurrentSelection[0] as MusicItem : null;
            EditBtn.IsEnabled = _selectedItem is not null;
        }
    }
    
}