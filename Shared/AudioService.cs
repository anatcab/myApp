using Microsoft.Maui.ApplicationModel;

namespace fApp.Shared;

public sealed class AudioService : IAudioService
{
#if ANDROID
    private Android.Media.SoundPool? _pool;
    private readonly Dictionary<string, int> _soundIds = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _initLock = new(1, 1);
#endif

    public async Task PlayAsync(string assetNameWithoutExtension, CancellationToken ct)
    {
#if ANDROID
        await EnsureLoadedAsync(assetNameWithoutExtension, ct);

        if (_pool is null) return;
        if (!_soundIds.TryGetValue(assetNameWithoutExtension, out var id)) return;

        _pool.Play(id, 1f, 1f, 1, 0, 1f);
        return;
#else
        await Task.CompletedTask;
#endif
    }

    public static void VibrateStart()
    {
        try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(35)); } catch { }
    }

#if ANDROID
    private async Task EnsureLoadedAsync(string key, CancellationToken ct)
    {
        if (_soundIds.ContainsKey(key)) return;

        await _initLock.WaitAsync(ct);
        try
        {
            if (_soundIds.ContainsKey(key)) return;

            _pool ??= new Android.Media.SoundPool.Builder()
                .SetMaxStreams(4)
                .Build();

            var fileName = $"{key}.wav";
            var cachePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            if (!File.Exists(cachePath))
            {
                using var src = await FileSystem.OpenAppPackageFileAsync(fileName);
                using var dst = File.Create(cachePath);
                await src.CopyToAsync(dst, ct);
            }

            var id = _pool.Load(cachePath, 1);
            if (id != 0) _soundIds[key] = id;
        }
        finally
        {
            _initLock.Release();
        }
    }
#endif
}
