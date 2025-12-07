using MauiMacApp.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input; 

namespace MauiMacApp.ViewModels;

public class EditPageViewModel : INotifyPropertyChanged
{
    private MusicItem _originalItem;
    
    public string Name { get; set; }
    public string Author { get; set; }
    public string Genre { get; set; }
    
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    private readonly TaskCompletionSource<MusicItem?> _tcs = new();
    public Task<MusicItem?> CompletionTask => _tcs.Task;

    public EditPageViewModel(MusicItem item)
    {
        _originalItem = item;

        Name = item.Name;
        Author = item.Author;
        Genre = item.Genre;

        SaveCommand = new Command(OnSave);
        CancelCommand = new Command(OnCancel);
    }

    private void OnSave()
    {
        var edited = new MusicItem
        {
            Name = this.Name.Trim(),
            Author = this.Author.Trim(),
            Genre = this.Genre.Trim()
        };
        Complete(edited);
    }

    private void OnCancel()
    {
        Complete(null);
    }

    public void Complete(MusicItem? result)
    {
        if (_tcs.Task.IsCompleted) return;
        _tcs.TrySetResult(result);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}