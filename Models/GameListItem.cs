namespace fApp;

public sealed record GameListItem(
    string Id,
    string DisplayNameKey,
    bool HasSetup,
    bool HasStats
);
