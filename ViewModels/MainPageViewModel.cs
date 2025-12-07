using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Storage;
using MauiMacApp.Models;
using MauiMacApp.Services;

namespace MauiMacApp.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    private readonly IEditDialogService _editDialogService;

    public ObservableCollection<MusicItem> Items { get; } = new();

    private MusicItem? _selectedItem;
    public MusicItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem == value) return;
            _selectedItem = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsEditEnabled));
            ((Command)EditCommand).ChangeCanExecute();
        }
    }

    private string _statusText = "Ready";
    public string StatusText
    {
        get => _statusText;
        set { if (_statusText == value) return; _statusText = value; OnPropertyChanged(); }
    }

    private string _resultText = string.Empty;
    public string ResultText
    {
        get => _resultText;
        set { if (_resultText == value) return; _resultText = value; OnPropertyChanged(); }
    }

    public bool IsEditEnabled => SelectedItem is not null;

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand EditCommand { get; }

    public Window? OwnerWindow { get; set; }

    public MainPageViewModel(IEditDialogService editDialogService)
    {
        _editDialogService = editDialogService;

        Items.Add(new MusicItem { Name = "Bohemian Rhapsody", Author = "Queen", Genre = "Rock" });
        Items.Add(new MusicItem { Name = "Imagine", Author = "John Lennon", Genre = "Pop" });
        Items.Add(new MusicItem { Name = "Billie Jean", Author = "Michael Jackson", Genre = "Pop" });

        LoadCommand = new Command(async () => await OnLoadAsync());
        SaveCommand = new Command(async () => await OnSaveAsync());
        EditCommand = new Command(async () => await OnEditAsync(), () => SelectedItem is not null);
    }

    private async Task OnLoadAsync()
    {
        try
        {
            StatusText = "Opening file picker...";

            var options = new PickOptions { PickerTitle = "Select a file" };
            var result = await FilePicker.Default.PickAsync(options);

            if (result != null)
            {
                var folderPath = Path.GetDirectoryName(result.FullPath);
                StatusText = "File selected";
                ResultText = $"Selected file: {result.FileName}\nFolder: {folderPath}";

                if (!string.IsNullOrEmpty(folderPath))
                {
                    var files = Directory.GetFiles(folderPath);
                    ResultText += $"\nFiles in folder: {files.Length}";
                }
            }
            else
            {
                StatusText = "No file selected";
                ResultText = string.Empty;
            }
        }
        catch (Exception ex)
        {
            StatusText = "Error";
            ResultText = $"Error: {ex.Message}";
        }
    }

    private async Task OnSaveAsync()
    {
        try
        {
            StatusText = "Saving file...";

            string fileName = "document.txt";
            string content = "Hello from MAUI macOS App!\nCreated on: " + DateTime.Now.ToString();

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            var saveResult = await FileSaver.Default.SaveAsync(fileName, stream, CancellationToken.None);

            if (saveResult.IsSuccessful)
            {
                StatusText = "File saved!";
                ResultText = $"File saved to:\n{saveResult.FilePath}";
            }
            else
            {
                StatusText = "Save canceled";
                ResultText = saveResult.Exception is null
                    ? "User canceled the save dialog."
                    : $"Error: {saveResult.Exception.Message}";
            }
        }
        catch (Exception ex)
        {
            StatusText = "Error";
            ResultText = $"Error: {ex.Message}";
        }
    }

    private async Task OnEditAsync()
    {
        if (SelectedItem is null || OwnerWindow is null)
            return;

        var edited = await _editDialogService.ShowEditDialogAsync(SelectedItem, OwnerWindow);

        if (edited is null)
        {
            StatusText = "Edit canceled";
            return;
        }

        var index = Items.IndexOf(SelectedItem);
        if (index >= 0)
        {
            Items[index] = edited;
            SelectedItem = edited;
            StatusText = "Row updated";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
