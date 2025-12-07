using MauiMacApp.Models;

namespace MauiMacApp.Views;

public partial class EditPage : ContentPage
{
    private readonly TaskCompletionSource<MusicItem?> _tcs = new();
    public Task<MusicItem?> CompletionTask => _tcs.Task;
    private readonly MusicItem _original;
    private bool _completed;

    public EditPage(MusicItem item)
    {
        InitializeComponent();
        _original = item;

        NameEntry.Text = item.Name;
        AuthorEntry.Text = item.Author;
        GenreEntry.Text = item.Genre;

        this.Disappearing += (s, e) => Complete(null);

        this.Loaded += (s, e) =>
        {
            if (this.Window != null)
            {
                this.Window.Destroying += (ws, we) => Complete(null);
            }
        };
    }

    private void Complete(MusicItem? result)
    {
        if (_completed) return;
        _completed = true;
        _tcs.TrySetResult(result);
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
        Complete(null);
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

        Complete(edited);
        CloseThisWindow();
    }
}