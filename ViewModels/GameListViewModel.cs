using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;
using System.Reflection;

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
        var prop = typeof(AppResources).GetProperty(key, BindingFlags.Public | BindingFlags.Static);
        if (prop is not null && prop.PropertyType == typeof(string))
        {
            var value = (string?)prop.GetValue(null);
            if (!string.IsNullOrWhiteSpace(value)) return value!;
        }

        var rmProp = typeof(AppResources).GetProperty("ResourceManager", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        var rm = rmProp?.GetValue(null) as System.Resources.ResourceManager;
        var s = rm?.GetString(key, CultureInfo.CurrentUICulture);
        return string.IsNullOrWhiteSpace(s) ? key : s!;
    }
}
