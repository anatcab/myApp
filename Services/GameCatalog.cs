namespace fApp;

public sealed class GameCatalog : IGameCatalog
{
    private static readonly IReadOnlyList<GameListItem> _games =
        new List<GameListItem>
        {
            new(
                Id: "music-counter",
                DisplayNameKey: "Game_MusicCounter",
                HasSetup: true,
                HasStats: true
            ),
            new(
                Id: "endure-intervals",
                DisplayNameKey: "Game_EndureIntervals",
                HasSetup: true,
                HasStats: true
            )
        };

    public IReadOnlyList<GameListItem> GetGames() => _games;
}
