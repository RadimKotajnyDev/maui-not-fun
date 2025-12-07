using MauiMacApp.Models;
using MauiMacApp.Views;

namespace MauiMacApp.Services;

public class EditDialogService : IEditDialogService
{
    public async Task<MusicItem?> ShowEditDialogAsync(MusicItem item, Window owner)
    {
        var editPage = new EditPage(item);

        double newWidth = 200;
        double newHeight = 200;

        double centerX = owner.X + (owner.Width - newWidth) / 2;
        double centerY = owner.Y + (owner.Height - newHeight) / 2;

        var newWindow = new Window(editPage)
        {
            Title = "Edit Music Item",
            Width = newWidth,
            Height = newHeight,
            X = centerX,
            Y = centerY
        };

        newWindow.Created += (s, e) =>
        {
            var win = (Window)s!;
            win.X = centerX;
            win.Y = centerY;
        };

        Application.Current?.OpenWindow(newWindow);

        var result = await editPage.CompletionTask;
        return result;
    }
}
