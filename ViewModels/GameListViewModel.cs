using CommunityToolkit.Mvvm.ComponentModel;

namespace fApp;

public partial class GamesListViewModel : ObservableObject
{
    private readonly IGameCatalog _catalog;

    public IReadOnlyList<GameListItem> Games => _catalog.GetGames();

    [ObservableProperty]
    private GameListItem? selectedGame;

    public GamesListViewModel(IGameCatalog catalog)
    {
        _catalog = catalog;
    }

    partial void OnSelectedGameChanged(GameListItem? value)
    {
        if (value is null) return;

        Application.Current?.MainPage?.DisplayAlert("Wybrano grę", value.DisplayName, "OK");
        SelectedGame = null;
    }
}
