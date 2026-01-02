namespace fApp.Shared;

public interface IRandomTimeProvider
{
    int NextSeconds(TimeRange range);
}
