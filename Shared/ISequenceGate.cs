namespace fApp.Shared;

public interface ISequenceGate
{
    bool IsBusy { get; }
    DateTimeOffset? BusyUntilUtc { get; }
    Task RunAsync(Func<CancellationToken, Task> sequence, TimeSpan cooldown, CancellationToken ct);
}
