using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using fApp.Shared;

namespace fApp.Games.EndureIntervals;

public partial class EndureIntervalsGameViewModel : ObservableObject
{
    private static readonly TimeSpan SequenceCooldown = TimeSpan.FromSeconds(5);

    private readonly IRandomTimeProvider _rng;
    private readonly ISequenceGate _gate;
    private readonly IAudioService _audio;
    [ObservableProperty] private bool _isTimer2Visible;
    [ObservableProperty] private double _timerCircleSize = 260;

    private EndureIntervalsConfig? _cfg;

    private readonly List<EndureIntervalsScenario> _scenarios =
    [
        new EndureIntervalsScenario(1, 4, 6, 4, "pop_short"),
        new EndureIntervalsScenario(2, 8, 10, 10, "pop_med"),
        new EndureIntervalsScenario(3, 12, 15, 15, "pop_long"),
    ];

    private CancellationTokenSource? _cts;

    private bool _isRunning;
    private DateTimeOffset _lastTickUtc;

    private int _t1LastDrawSeconds;
    private DateTimeOffset _t1DueUtc;
    private bool _t1Pending;
    private bool _t1LimitArmed;
    private int _t1RepeatCount;
    private int _t1MaxRepeats;

    private int _t2LastDrawSeconds;
    private DateTimeOffset _t2DueUtc;
    private bool _t2Pending;

    private int _timer1MaxDurationSeconds;
    private int _timer1MaxDurationCount;

    private int _timer2SequenceCount;
    private int _timer2DelayAfter2SumSeconds;
    private int _timer2BaseAfter3SumSeconds;

    [ObservableProperty] private string _timer1RemainingText = "";
    [ObservableProperty] private string _timer2RemainingText = "";
    [ObservableProperty] private string _timer1RepeatText = "";
    [ObservableProperty] private string _timer2EnabledText = "";
    [ObservableProperty] private string _statusText = "";
    [ObservableProperty] private string _startPauseText = "";

    public EndureIntervalsGameViewModel(IRandomTimeProvider rng, ISequenceGate gate, IAudioService audio)
    {
        _rng = rng;
        _gate = gate;
        _audio = audio;
        StartPauseText = EndureIntervalsResources.Game_Start;
    }

    public void Initialize(EndureIntervalsConfig cfg)
    {
        _cfg = cfg;

        IsTimer2Visible = cfg.Timer2Enabled;

        _t1MaxRepeats = cfg.Timer1MaxRepeats;
        _t1RepeatCount = 0;
        _t1LimitArmed = false;

        _timer1MaxDurationSeconds = 0;
        _timer1MaxDurationCount = 0;

        _timer2SequenceCount = 0;
        _timer2DelayAfter2SumSeconds = 0;
        _timer2BaseAfter3SumSeconds = 0;

        _t1Pending = false;
        _t2Pending = false;

        _t1LastDrawSeconds = DrawTimer1();
        _t1DueUtc = DateTimeOffset.UtcNow.AddSeconds(_t1LastDrawSeconds);

        _t2LastDrawSeconds = cfg.Timer2Enabled ? DrawTimer2() : 0;
        _t2DueUtc = DateTimeOffset.UtcNow.AddSeconds(cfg.Timer2Enabled ? _t2LastDrawSeconds : 999999);

        _isRunning = false;
        UpdateUi();

        StatusText = "Ready";
        Timer2EnabledText = cfg.Timer2Enabled ? "Enabled" : "Disabled";
    }

    private int DrawTimer1()
    {
        if (_cfg is null) return 0;
        return _rng.NextSeconds(_cfg.Timer1Range);
    }

    private int DrawTimer2()
    {
        if (_cfg is null) return 0;
        return _rng.NextSeconds(_cfg.Timer2Range);
    }

    [RelayCommand]
    private void StartPause()
    {
        if (_cfg is null) return;

        if (_isRunning)
        {
            PauseInternal();
            return;
        }

        StartInternal();
    }

    private void StartInternal()
    {
        if (_cfg is null) return;

        AudioService.VibrateStart();

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        _isRunning = true;
        _lastTickUtc = DateTimeOffset.UtcNow;
        StartPauseText = EndureIntervalsResources.Game_Pause;

        StatusText = "Running";

        Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
        {
            if (_cts is null || _cts.IsCancellationRequested) return false;
            Tick(_cts.Token);
            return true;
        });
    }

    private void PauseInternal()
    {
        _isRunning = false;
        _cts?.Cancel();
        _cts = null;
        StartPauseText = EndureIntervalsResources.Game_Start;
        StatusText = "Paused";
        UpdateUi();
    }

    [RelayCommand]
    private void Reset()
    {
        if (_cfg is null) return;

        _t1RepeatCount = 0;
        _t1LimitArmed = false;

        if (_cfg.Timer2Enabled)
        {
            _t2DueUtc = DateTimeOffset.UtcNow.AddSeconds(_t2LastDrawSeconds);
            _t2Pending = false;
        }

        StatusText = "Reset";
        UpdateUi();
    }

    [RelayCommand]
    private async Task End()
    {
        PauseInternal();

        var snap = new EndureIntervalsStatsSnapshot(
            Timer1MaxDurationSeconds: _timer1MaxDurationSeconds,
            Timer1MaxDurationCount: _timer1MaxDurationCount,
            Timer2SequenceCount: _timer2SequenceCount,
            Timer2DelayAfter2SumSeconds: _timer2DelayAfter2SumSeconds,
            Timer2BaseAfter3SumSeconds: _timer2BaseAfter3SumSeconds
        );

        var stats = Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<EndureIntervalsStatsPage>();
        var vm = (EndureIntervalsStatsViewModel)stats.BindingContext;
        vm.SetData(snap);

        await Application.Current!.MainPage!.Navigation.PushAsync(stats);
    }

    private void Tick(CancellationToken ct)
    {
        if (_cfg is null) return;
        if (!_isRunning) return;

        var now = DateTimeOffset.UtcNow;
        _lastTickUtc = now;

        if (!_t1Pending && now >= _t1DueUtc)
            _t1Pending = true;

        if (_cfg.Timer2Enabled && !_t2Pending && now >= _t2DueUtc)
            _t2Pending = true;

        TryRunPending(ct);
        UpdateUi();
    }

    private void TryRunPending(CancellationToken ct)
    {
        if (_cfg is null) return;

        if (_t1Pending && !_gate.IsBusy)
        {
            _t1Pending = false;
            _ = RunTimer1SequenceAsync(ct);
            return;
        }

        if (_cfg.Timer2Enabled && _t2Pending && !_gate.IsBusy)
        {
            _t2Pending = false;
            _ = RunTimer2SequenceAsync(ct);
            return;
        }
    }

    private async Task RunTimer1SequenceAsync(CancellationToken outerCt)
    {
        if (_cfg is null) return;

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(outerCt);
        var ct = linked.Token;

        var duration = _t1LastDrawSeconds;
        TrackTimer1Duration(duration);

        await _gate.RunAsync(async token =>
        {
            if (_t1LimitArmed)
            {
                await _audio.PlayAsync("inf_limit", token);
                _t1LimitArmed = false;
                _t1RepeatCount = 0;
                _t1MaxRepeats += 1;
            }
            else
            {
                await _audio.PlayAsync("inf_tick", token);
                _t1RepeatCount += 1;
                if (_t1RepeatCount >= _t1MaxRepeats)
                    _t1LimitArmed = true;
            }
        }, SequenceCooldown, ct);

        if (_cfg is null) return;

        _t1LastDrawSeconds = DrawTimer1();
        _t1DueUtc = DateTimeOffset.UtcNow.AddSeconds(_t1LastDrawSeconds);
    }

    private async Task RunTimer2SequenceAsync(CancellationToken outerCt)
    {
        if (_cfg is null) return;

        var scenario = PickScenario();

        _timer2SequenceCount += 1;
        _timer2DelayAfter2SumSeconds += scenario.DelayAfterSecondSeconds;
        _timer2BaseAfter3SumSeconds += scenario.BaseAfterThirdSeconds;

        var rand = scenario.MaxRandomAfterThirdSeconds <= 0 ? 0 : Random.Shared.Next(0, scenario.MaxRandomAfterThirdSeconds + 1);

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(outerCt);
        var ct = linked.Token;

        await _gate.RunAsync(async token =>
        {
            await _audio.PlayAsync("pop_warn", token);
            await _audio.PlayAsync(scenario.ScenarioBeepAsset, token);
            await Task.Delay(TimeSpan.FromSeconds(scenario.DelayAfterSecondSeconds), token);
            await _audio.PlayAsync("pop_wait", token);
            await Task.Delay(TimeSpan.FromSeconds(scenario.BaseAfterThirdSeconds + rand), token);
            await _audio.PlayAsync("pop_end", token);
        }, SequenceCooldown, ct);

        if (_cfg is null) return;

        _t2LastDrawSeconds = DrawTimer2();
        _t2DueUtc = DateTimeOffset.UtcNow.AddSeconds(_t2LastDrawSeconds);
    }

    private EndureIntervalsScenario PickScenario()
    {
        var idx = Random.Shared.Next(0, _scenarios.Count);
        return _scenarios[idx];
    }

    private void TrackTimer1Duration(int seconds)
    {
        if (seconds > _timer1MaxDurationSeconds)
        {
            _timer1MaxDurationSeconds = seconds;
            _timer1MaxDurationCount = 1;
            return;
        }

        if (seconds == _timer1MaxDurationSeconds)
            _timer1MaxDurationCount += 1;
    }

    private void UpdateUi()
    {
        var now = DateTimeOffset.UtcNow;

        var t1rem = Math.Max(0, (int)Math.Ceiling((_t1DueUtc - now).TotalSeconds));
        Timer1RemainingText = $"{t1rem}s";
        Timer1RepeatText = _t1LimitArmed
            ? $"Limit armed ({_t1RepeatCount}/{_t1MaxRepeats})"
            : $"{_t1RepeatCount}/{_t1MaxRepeats}";

        if (_cfg is null || !_cfg.Timer2Enabled)
        {
            Timer2RemainingText = "-";
        }
        else
        {
            var t2rem = Math.Max(0, (int)Math.Ceiling((_t2DueUtc - now).TotalSeconds));
            Timer2RemainingText = $"{t2rem}s";
        }

        StartPauseText = _isRunning ? EndureIntervalsResources.Game_Pause : EndureIntervalsResources.Game_Start;

        if (_gate.IsBusy)
        {
            StatusText = "Sequence running";
            return;
        }

        if (_t1Pending || _t2Pending)
        {
            StatusText = "Pending sequence";
            return;
        }

        StatusText = _isRunning ? "Running" : "Paused";
    }
}
