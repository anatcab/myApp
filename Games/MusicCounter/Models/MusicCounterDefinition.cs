namespace fApp.Games.MusicCounter;

public sealed record MusicCounterDefinition(IReadOnlyList<MusicCounterCategoryDefinition> Categories);

public sealed record MusicCounterCategoryDefinition(
    string Id,
    string NameKey,
    IReadOnlyList<MusicCounterPhraseDefinition> Phrases
);

public sealed record MusicCounterPhraseDefinition(
    string Id,
    string TextKey
);
