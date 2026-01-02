namespace fApp.Shared;

public readonly record struct TimeRange(int MinSeconds, int MaxSeconds)
{
    public bool IsValid => MinSeconds > 0 && MaxSeconds > 0 && MinSeconds <= MaxSeconds;
}
