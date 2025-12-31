namespace fApp.Shared;

public sealed class NonRepeatingRandom : INonRepeatingRandom
{
    private readonly Random _rng = new();

    public bool TryTake<T>(IList<T> pool, out T? value)
    {
        if (pool.Count == 0)
        {
            value = default;
            return false;
        }

        var idx = _rng.Next(0, pool.Count);
        value = pool[idx];
        pool.RemoveAt(idx);
        return true;
    }
}
