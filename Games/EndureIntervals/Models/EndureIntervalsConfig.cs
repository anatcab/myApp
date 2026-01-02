using fApp.Shared;

namespace fApp.Games.EndureIntervals;

public sealed record EndureIntervalsConfig(
    TimeRange Timer1Range,
    int Timer1MaxRepeats,
    bool Timer2Enabled,
    TimeRange Timer2Range
);
