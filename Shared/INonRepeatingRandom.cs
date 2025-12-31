namespace fApp.Shared;

public interface INonRepeatingRandom
{
    bool TryTake<T>(IList<T> pool, out T? value);
}
