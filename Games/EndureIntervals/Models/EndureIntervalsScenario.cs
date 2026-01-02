namespace fApp.Games.EndureIntervals;

public sealed record EndureIntervalsScenario(
    int Id,
    int DelayAfterSecondSeconds,
    int BaseAfterThirdSeconds,
    int MaxRandomAfterThirdSeconds,
    string ScenarioBeepAsset
);
