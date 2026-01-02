namespace fApp.Shared;

public interface IAudioService
{
    Task PlayAsync(string assetNameWithoutExtension, CancellationToken ct);
}
