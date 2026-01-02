namespace fApp.Shared;

public sealed class RandomTimeProvider : IRandomTimeProvider
{
    private readonly Random _rng = new();

    public int NextSeconds(TimeRange range)
    {
        if (!range.IsValid) return range.MinSeconds;
        if (range.MinSeconds == range.MaxSeconds) return range.MinSeconds;
        return _rng.Next(range.MinSeconds, range.MaxSeconds + 1);
    }
}
