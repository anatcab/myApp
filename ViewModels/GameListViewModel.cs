using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;

namespace fApp;

public partial class GamesListViewModel : ObservableObject
{
    private readonly IGameCatalog _catalog;

    public IReadOnlyList<GameListEntry> Games { get; }

    public GamesListViewModel(IGameCatalog catalog)
    {
        _catalog = catalog;
        Games = _catalog.GetGames()
            .Select(g => new GameListEntry(g, GetResource(g.DisplayNameKey)))
            .ToList();
    }

    [RelayCommand]
    private async Task SelectGame(GameListEntry entry)
    {
        await Application.Current!.MainPage!.DisplayAlert("Wybrano grę", entry.DisplayName, "OK");
    }

    private static string GetResource(string key)
    {
        var s = AppResources.ResourceManager.GetString(key, CultureInfo.CurrentUICulture);
        return string.IsNullOrWhiteSpace(s) ? key : s!;
    }
}
