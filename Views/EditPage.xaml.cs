using MauiMacApp.Models;

namespace MauiMacApp.Views;

public partial class EditPage : ContentPage
{
    private readonly TaskCompletionSource<MusicItem?> _tcs = new();
    public Task<MusicItem?> CompletionTask => _tcs.Task;
    private readonly MusicItem _original;

    public EditPage(MusicItem item)
    {
        InitializeComponent();
        _original = item;

        NameEntry.Text = item.Name;
        AuthorEntry.Text = item.Author;
        GenreEntry.Text = item.Genre;
        
        this.Unloaded += (s, e) => _tcs.TrySetResult(null);
    }

    private void CloseThisWindow()
    {
        if (this.Window != null)
        {
            Application.Current?.CloseWindow(this.Window);
        }
    }

    private void OnCancelClicked(object? sender, System.EventArgs e)
    {
        _tcs.TrySetResult(null);
        CloseThisWindow();
    }

    private void OnSaveClicked(object? sender, System.EventArgs e)
    {
        var edited = new MusicItem
        {
            Name = NameEntry.Text?.Trim() ?? string.Empty,
            Author = AuthorEntry.Text?.Trim() ?? string.Empty,
            Genre = GenreEntry.Text?.Trim() ?? string.Empty
        };

        _tcs.TrySetResult(edited);
        CloseThisWindow();
    }
}