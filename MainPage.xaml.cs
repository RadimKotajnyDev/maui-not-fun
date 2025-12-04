using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Storage;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MauiMacApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
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
    }
}