using MauiMacApp.Models;
using MauiMacApp.ViewModels; 

namespace MauiMacApp.Views;

public partial class EditPage : ContentPage
{
    private readonly EditPageViewModel _viewModel;

    public Task<MusicItem?> CompletionTask => _viewModel.CompletionTask;

    public EditPage(MusicItem item)
    {
        InitializeComponent();
        
        _viewModel = new EditPageViewModel(item);
        this.BindingContext = _viewModel;

        _viewModel.CompletionTask.ContinueWith(t =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CloseThisWindow();
            });
        });

        this.Disappearing += (s, e) => _viewModel.Complete(null);

        this.Loaded += (s, e) =>
        {
            if (this.Window != null)
            {
                this.Window.Destroying += (ws, we) => _viewModel.Complete(null);
            }
        };
    }

    private void CloseThisWindow()
    {
        if (this.Window != null)
        {
            Application.Current?.CloseWindow(this.Window);
        }
    }
}