namespace fApp;

public sealed record GameListItem(
    string Id,
    string DisplayName,
    string Description,
    bool HasSetup,
    bool HasStats
);
