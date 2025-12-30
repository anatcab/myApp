namespace fApp;

public sealed class GameCatalog : IGameCatalog
{
    private static readonly IReadOnlyList<GameListItem> _games =
        new List<GameListItem>
        {
            new(
                Id: "phrases-counter",
                DisplayName: "Liczenie fraz",
                Description: "Losuj frazy z puli i zliczaj ich wystąpienia przyciskami.",
                HasSetup: true,
                HasStats: true
            ),
            new(
                Id: "audio-intervals",
                DisplayName: "Sygnały w interwałach",
                Description: "Jeden lub kilka sygnałów dźwiękowych w ustalonych odstępach.",
                HasSetup: true,
                HasStats: false
            )
        };

    public IReadOnlyList<GameListItem> GetGames() => _games;
}
