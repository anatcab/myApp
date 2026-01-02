namespace fApp.Shared;

public sealed class SequenceGate : ISequenceGate
{
    private readonly SemaphoreSlim _mutex = new(1, 1);

    public bool IsBusy { get; private set; }
    public DateTimeOffset? BusyUntilUtc { get; private set; }

    public async Task RunAsync(Func<CancellationToken, Task> sequence, TimeSpan cooldown, CancellationToken ct)
    {
        await _mutex.WaitAsync(ct);

        try
        {
            IsBusy = true;
            BusyUntilUtc = null;

            await sequence(ct);

            if (cooldown > TimeSpan.Zero)
            {
                BusyUntilUtc = DateTimeOffset.UtcNow + cooldown;
                await Task.Delay(cooldown, ct);
            }
        }
        finally
        {
            IsBusy = false;
            BusyUntilUtc = null;
            _mutex.Release();
        }
    }
}
