using MauiMacApp.Models;

namespace MauiMacApp.Services;

public interface IEditDialogService
{
    Task<MusicItem?> ShowEditDialogAsync(MusicItem item, Window owner);
}
