using Microsoft.Maui.Controls;
using System;
using MauiMacApp.ViewModels;
using MauiMacApp.Services;

namespace MauiMacApp.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _viewModel;

        public MainPage()
        {
            InitializeComponent();

            // For simplicity, instantiate VM and service directly.
            _viewModel = new MainPageViewModel(new EditDialogService());
            BindingContext = _viewModel;

            this.Loaded += (s, e) =>
            {
                CenterMainWindowOnCreation();
                // Provide window reference to VM for dialog centering
                _viewModel.OwnerWindow = this.Window;
            };
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
    }
    
}