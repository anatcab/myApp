using Microsoft.Maui.ApplicationModel;

namespace fApp.Shared;

public sealed class AudioService : IAudioService
{
#if ANDROID
    private Android.Media.SoundPool? _pool;
    private readonly Dictionary<string, int> _soundIds = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Task> _loadTasks = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim _initLock = new(1, 1);

    private sealed class LoadListener : Java.Lang.Object, Android.Media.SoundPool.IOnLoadCompleteListener
    {
        private readonly Dictionary<int, TaskCompletionSource<bool>> _pending;

        public LoadListener(Dictionary<int, TaskCompletionSource<bool>> pending) => _pending = pending;

        public void OnLoadComplete(Android.Media.SoundPool? soundPool, int sampleId, int status)
        {
            if (!_pending.TryGetValue(sampleId, out var tcs)) return;
            _pending.Remove(sampleId);
            if (status == 0) tcs.TrySetResult(true);
            else tcs.TrySetException(new InvalidOperationException($"SoundPool load failed: id={sampleId}, status={status}"));
        }
    }

    private readonly Dictionary<int, TaskCompletionSource<bool>> _pendingLoads = new();
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
        Task? loadTask;

        await _initLock.WaitAsync(ct);
        try
        {
            if (_soundIds.ContainsKey(key)) return;

            if (_loadTasks.TryGetValue(key, out loadTask))
            {
            }
            else
            {
                loadTask = LoadAsync(key, ct);
                _loadTasks[key] = loadTask;
            }
        }
        finally
        {
            _initLock.Release();
        }

        await loadTask;
    }

    private async Task LoadAsync(string key, CancellationToken ct)
    {
        _pool ??= new Android.Media.SoundPool.Builder()
            .SetMaxStreams(4)
            .Build();

        _pool.SetOnLoadCompleteListener(new LoadListener(_pendingLoads));

        var fileName = $"{key}.wav";
        var cachePath = Path.Combine(FileSystem.CacheDirectory, fileName);

        using (var src = await FileSystem.OpenAppPackageFileAsync(fileName))
        {
            using var dst = File.Create(cachePath);
            await src.CopyToAsync(dst, ct);
        }

        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var id = _pool.Load(cachePath, 1);

        if (id == 0)
            throw new InvalidOperationException($"SoundPool returned id=0 for {fileName}");

        _pendingLoads[id] = tcs;

        await tcs.Task;

        await _initLock.WaitAsync(ct);
        try
        {
            _soundIds[key] = id;
        }
        finally
        {
            _initLock.Release();
        }
    }
#endif
}
