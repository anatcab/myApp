namespace fApp;

public interface IGameCatalog
{
    IReadOnlyList<GameListItem> GetGames();
}
