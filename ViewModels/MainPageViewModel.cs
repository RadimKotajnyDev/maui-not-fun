using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Devices;
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
            StatusText = "Opening CSV file...";

            var csvFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.MacCatalyst, new[] { "public.comma-separated-values-text", ".csv" } },
                { DevicePlatform.iOS, new[] { "public.comma-separated-values-text", ".csv" } },
                { DevicePlatform.WinUI, new[] { ".csv" } },
                { DevicePlatform.Android, new[] { "text/csv" } },
                { DevicePlatform.Tizen, new[] { ".csv" } }
            });

            var options = new PickOptions
            {
                PickerTitle = "Select CSV file",
                FileTypes = csvFileType
            };
            var result = await FilePicker.Default.PickAsync(options);

            if (result == null)
            {
                StatusText = "No file selected";
                ResultText = string.Empty;
                return;
            }

            string csvContent;
            using (var stream = await result.OpenReadAsync())
            using (var reader = new StreamReader(stream))
            {
                csvContent = await reader.ReadToEndAsync();
            }

            var loaded = DeserializeFromCsv(csvContent);

            Items.Clear();
            foreach (var item in loaded)
                Items.Add(item);

            SelectedItem = null;
            ((Command)EditCommand).ChangeCanExecute();

            StatusText = "CSV loaded";
            ResultText = $"Loaded {loaded.Count} item(s) from {result.FileName}";
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
            StatusText = "Saving CSV...";

            string fileName = $"music_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string content = SerializeToCsv(Items);

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            var saveResult = await FileSaver.Default.SaveAsync(fileName, stream, CancellationToken.None);

            if (saveResult.IsSuccessful)
            {
                StatusText = "CSV saved";
                ResultText = $"Saved {Items.Count} item(s) to:\n{saveResult.FilePath}";
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

    private static string SerializeToCsv(IEnumerable<MusicItem> items)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Name,Author,Genre");
        foreach (var it in items)
        {
            sb.Append(EscapeCsv(it.Name)).Append(',')
              .Append(EscapeCsv(it.Author)).Append(',')
              .Append(EscapeCsv(it.Genre)).AppendLine();
        }
        return sb.ToString();
    }

    private static string EscapeCsv(string value)
    {
        value ??= string.Empty;
        bool mustQuote = value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r');
        if (value.Contains('"'))
            value = value.Replace("\"", "\"\"");
        return mustQuote ? $"\"{value}\"" : value;
    }

    private static List<MusicItem> DeserializeFromCsv(string csv)
    {
        var list = new List<MusicItem>();
        if (string.IsNullOrWhiteSpace(csv))
            return list;

        using var reader = new StringReader(csv);
        string? line;
        bool headerSkipped = false;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = ParseCsvLine(line);

            if (!headerSkipped)
            {
                headerSkipped = true;
                if (fields.Count >= 3 &&
                    string.Equals(fields[0], "Name", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(fields[1], "Author", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(fields[2], "Genre", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            while (UnbalancedQuotes(fields))
            {
                var next = reader.ReadLine();
                if (next is null) break;
                line += "\n" + next;
                fields = ParseCsvLine(line);
            }

            if (fields.Count == 0) continue;

            string name = fields.ElementAtOrDefault(0) ?? string.Empty;
            string author = fields.ElementAtOrDefault(1) ?? string.Empty;
            string genre = fields.ElementAtOrDefault(2) ?? string.Empty;
            list.Add(new MusicItem { Name = name, Author = author, Genre = genre });
        }
        return list;
    }

    private static bool UnbalancedQuotes(List<string> fields)
    {
        return false;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        if (line is null)
            return result;

        var sb = new System.Text.StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false; 
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            else
            {
                if (c == ',')
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                }
                else if (c == '"')
                {
                    inQuotes = true;
                }
                else
                {
                    sb.Append(c);
                }
            }
        }
        result.Add(sb.ToString());
        return result;
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
