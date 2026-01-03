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

    private EndureIntervalsConfig? _cfg;

    private readonly List<EndureIntervalsScenario> _scenarios =
    [
        new EndureIntervalsScenario(1, 4, 6, 4, "pop_warn_s1"),
        new EndureIntervalsScenario(2, 8, 10, 10, "pop_warn_s2"),
        new EndureIntervalsScenario(3, 12, 15, 15, "pop_warn_s3"),
    ];

    private CancellationTokenSource? _cts;
    private bool _isRunning;

    private int _t1LastDrawSeconds;
    private DateTimeOffset _t1DueUtc;
    private bool _t1Pending;
    private bool _t1LimitArmed;
    private int _t1RepeatCount;
    private int _t1MaxRepeats;

    private int _t2LastDrawSeconds;
    private DateTimeOffset _t2DueUtc;
    private bool _t2Pending;

    private int _timer1MaxRepeatsReached;

    private int _timer2SequenceCount;
    private int _timer2DelayAfter2SumSeconds;
    private int _timer2BaseAfter3SumSeconds;

    private bool _t1InSequence;
    private bool _t2InSequence;

    private bool _t1FrozenForTimer2;
    private int _t1FrozenRemainingSeconds;

    private int _t1StoredRemainingSeconds;
    private int _t2StoredRemainingSeconds;

    private string? _t1OverrideText;
    private string? _t2OverrideText;

    [ObservableProperty] private string _timer1RemainingText = "";
    [ObservableProperty] private string _timer2RemainingText = "";
    [ObservableProperty] private string _startPauseText = "";
    [ObservableProperty] private bool _isTimer2Visible;
    [ObservableProperty] private double _timerCircleSize = 260;

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

        _timer1MaxRepeatsReached = 0;

        _timer2SequenceCount = 0;
        _timer2DelayAfter2SumSeconds = 0;
        _timer2BaseAfter3SumSeconds = 0;

        _t1Pending = false;
        _t2Pending = false;

        _t1InSequence = false;
        _t2InSequence = false;

        _t1FrozenForTimer2 = false;
        _t1FrozenRemainingSeconds = 0;

        _t1OverrideText = null;
        _t2OverrideText = null;

        _t1LastDrawSeconds = DrawTimer1();
        _t1StoredRemainingSeconds = _t1LastDrawSeconds;
        _t1DueUtc = DateTimeOffset.UtcNow.AddYears(10);

        _t2LastDrawSeconds = cfg.Timer2Enabled ? DrawTimer2() : 0;
        _t2StoredRemainingSeconds = cfg.Timer2Enabled ? _t2LastDrawSeconds : 0;
        _t2DueUtc = DateTimeOffset.UtcNow.AddYears(10);

        _isRunning = false;
        StartPauseText = EndureIntervalsResources.Game_Start;
        UpdateUi();
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
        VibrateIfEnabled(TimeSpan.FromMilliseconds(35));

        if (_cfg is null) return;

        var now = DateTimeOffset.UtcNow;

        if (!_t1InSequence && !_t1FrozenForTimer2)
            _t1DueUtc = now.AddSeconds(Math.Max(0, _t1StoredRemainingSeconds));

        if (_cfg.Timer2Enabled && !_t2InSequence)
            _t2DueUtc = now.AddSeconds(Math.Max(0, _t2StoredRemainingSeconds));

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var myCts = _cts;

        _isRunning = true;
        StartPauseText = EndureIntervalsResources.Game_Pause;

        Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
        {
            if (_cts != myCts) return false;
            if (myCts.IsCancellationRequested) return false;
            Tick(myCts.Token);
            return true;
        });

        UpdateUi();
    }

    private void PauseInternal()
    {
        if (_cfg is null) return;

        var now = DateTimeOffset.UtcNow;

        if (!_t1InSequence && !_t1FrozenForTimer2)
            _t1StoredRemainingSeconds = GetRemainingSeconds(_t1DueUtc, now);

        if (_cfg.Timer2Enabled && !_t2InSequence)
            _t2StoredRemainingSeconds = GetRemainingSeconds(_t2DueUtc, now);

        _isRunning = false;
        _cts?.Cancel();
        _cts = null;
        StartPauseText = EndureIntervalsResources.Game_Start;
        UpdateUi();
    }

    [RelayCommand]
    private void Reset()
    {
        if (_cfg is null) return;

        _t1Pending = false;
        _t2Pending = false;

        _t1InSequence = false;
        _t2InSequence = false;

        _t1FrozenForTimer2 = false;
        _t1FrozenRemainingSeconds = 0;

        _t1OverrideText = null;
        _t2OverrideText = null;

        _t1RepeatCount = 0;
        _t1LimitArmed = false;

        _t1LastDrawSeconds = DrawTimer1();
        _t1StoredRemainingSeconds = _t1LastDrawSeconds;

        if (_cfg.Timer2Enabled)
        {
            _t2LastDrawSeconds = DrawTimer2();
            _t2StoredRemainingSeconds = _t2LastDrawSeconds;
        }
        else
        {
            _t2LastDrawSeconds = 0;
            _t2StoredRemainingSeconds = 0;
        }

        if (_isRunning)
        {
            var now = DateTimeOffset.UtcNow;
            _t1DueUtc = now.AddSeconds(Math.Max(0, _t1StoredRemainingSeconds));
            _t2DueUtc = _cfg.Timer2Enabled ? now.AddSeconds(Math.Max(0, _t2StoredRemainingSeconds)) : now.AddYears(10);
        }
        else
        {
            _t1DueUtc = DateTimeOffset.UtcNow.AddYears(10);
            _t2DueUtc = DateTimeOffset.UtcNow.AddYears(10);
        }

        UpdateUi();
    }

    [RelayCommand]
    private async Task End()
    {
        PauseInternal();

        var snap = new EndureIntervalsStatsSnapshot(
            Timer1MaxRepeatsReached: _timer1MaxRepeatsReached,
            Timer2Enabled: _cfg?.Timer2Enabled == true,
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

        if (!_t1InSequence && !_t1FrozenForTimer2 && !_t1Pending && now >= _t1DueUtc)
            _t1Pending = true;

        if (_cfg.Timer2Enabled && !_t2InSequence && !_t2Pending && now >= _t2DueUtc)
            _t2Pending = true;

        TryRunPending(ct);
        UpdateUi();
    }

    private void TryRunPending(CancellationToken ct)
    {
        if (_cfg is null) return;

        if (_cfg.Timer2Enabled && _t2Pending && !_gate.IsBusy)
        {
            _t2Pending = false;
            _ = RunTimer2SequenceAsync(ct);
            return;
        }

        if (_t1Pending && !_gate.IsBusy)
        {
            _t1Pending = false;
            _ = RunTimer1SequenceAsync(ct);
            return;
        }
    }

    private async Task RunTimer1SequenceAsync(CancellationToken outerCt)
    {
        if (_cfg is null) return;

        _t1InSequence = true;

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(outerCt);
        var ct = linked.Token;

        await _gate.RunAsync(async token =>
        {
            if (_t1LimitArmed)
            {
                _t1OverrideText = EndureIntervalsResources.Game_Timer1_Reset;
                UpdateUi();

                VibrateIfEnabled(TimeSpan.FromMilliseconds(35));
                await _audio.PlayAsync("inf_limit", token);

                _t1LimitArmed = false;
                _t1RepeatCount = 0;
                _t1MaxRepeats += 1;
            }
            else
            {
                _t1OverrideText = EndureIntervalsResources.Game_Timer1_Go;
                UpdateUi();

                VibrateIfEnabled(TimeSpan.FromMilliseconds(35));
                await _audio.PlayAsync("inf_tick", token);

                _t1RepeatCount += 1;
                if (_t1RepeatCount > _timer1MaxRepeatsReached)
                    _timer1MaxRepeatsReached = _t1RepeatCount;

                if (_t1RepeatCount >= _t1MaxRepeats)
                    _t1LimitArmed = true;
            }
        }, SequenceCooldown, ct);

        if (_cfg is null) return;

        _t1InSequence = false;
        _t1OverrideText = null;

        _t1LastDrawSeconds = DrawTimer1();
        _t1StoredRemainingSeconds = _t1LastDrawSeconds;

        if (_isRunning && !_t1FrozenForTimer2)
            _t1DueUtc = DateTimeOffset.UtcNow.AddSeconds(_t1StoredRemainingSeconds);
        else
            _t1DueUtc = DateTimeOffset.UtcNow.AddYears(10);

        UpdateUi();
    }

    private async Task RunTimer2SequenceAsync(CancellationToken outerCt)
    {
        if (_cfg is null) return;

        FreezeTimer1ForTimer2();

        _t2InSequence = true;

        var scenario = PickScenario();

        _timer2SequenceCount += 1;
        _timer2DelayAfter2SumSeconds += scenario.DelayAfterSecondSeconds;
        _timer2BaseAfter3SumSeconds += scenario.BaseAfterThirdSeconds;

        var rand = scenario.MaxRandomAfterThirdSeconds <= 0 ? 0 : Random.Shared.Next(0, scenario.MaxRandomAfterThirdSeconds + 1);

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(outerCt);
        var ct = linked.Token;

        await _gate.RunAsync(async token =>
        {
            _t2OverrideText = EndureIntervalsResources.Game_PopWarn;
            UpdateUi();
            VibrateIfEnabled(TimeSpan.FromMilliseconds(35));
            await _audio.PlayAsync(scenario.ScenarioWarnAsset, token);
            await Task.Delay(TimeSpan.FromSeconds(6), token);

            _t2OverrideText = EndureIntervalsResources.Game_PopGo;
            UpdateUi();
            VibrateIfEnabled(TimeSpan.FromMilliseconds(35));
            await _audio.PlayAsync("pop_go", token);
            await Task.Delay(TimeSpan.FromSeconds(scenario.DelayAfterSecondSeconds), token);

            _t2OverrideText = EndureIntervalsResources.Game_PopWait;
            UpdateUi();
            VibrateIfEnabled(TimeSpan.FromMilliseconds(35));
            await _audio.PlayAsync("pop_wait", token);
            await Task.Delay(TimeSpan.FromSeconds(scenario.BaseAfterThirdSeconds + rand), token);

            _t2OverrideText = EndureIntervalsResources.Game_PopEnd;
            UpdateUi();
            VibrateIfEnabled(TimeSpan.FromMilliseconds(35));
            await _audio.PlayAsync("pop_end", token);
        }, SequenceCooldown, ct);

        if (_cfg is null) return;

        _t2InSequence = false;
        _t2OverrideText = null;

        _t2LastDrawSeconds = DrawTimer2();
        _t2StoredRemainingSeconds = _t2LastDrawSeconds;

        if (_isRunning)
            _t2DueUtc = DateTimeOffset.UtcNow.AddSeconds(_t2StoredRemainingSeconds);
        else
            _t2DueUtc = DateTimeOffset.UtcNow.AddYears(10);

        ResumeTimer1AfterTimer2Cooldown();

        UpdateUi();
    }

    private void FreezeTimer1ForTimer2()
    {
        if (_t1FrozenForTimer2) return;

        var now = DateTimeOffset.UtcNow;

        if (_isRunning && !_t1InSequence)
            _t1FrozenRemainingSeconds = GetRemainingSeconds(_t1DueUtc, now);
        else
            _t1FrozenRemainingSeconds = Math.Max(0, _t1StoredRemainingSeconds);

        _t1StoredRemainingSeconds = _t1FrozenRemainingSeconds;

        _t1FrozenForTimer2 = true;
        _t1OverrideText = EndureIntervalsResources.Game_Timer1_Wait;
        UpdateUi();
    }

    private void ResumeTimer1AfterTimer2Cooldown()
    {
        if (!_t1FrozenForTimer2) return;

        _t1FrozenForTimer2 = false;
        _t1OverrideText = null;

        if (_isRunning && !_t1InSequence)
            _t1DueUtc = DateTimeOffset.UtcNow.AddSeconds(Math.Max(0, _t1StoredRemainingSeconds));
        else
            _t1DueUtc = DateTimeOffset.UtcNow.AddYears(10);

        UpdateUi();
    }

    private static int GetRemainingSeconds(DateTimeOffset dueUtc, DateTimeOffset nowUtc)
    {
        return Math.Max(0, (int)Math.Ceiling((dueUtc - nowUtc).TotalSeconds));
    }

    private EndureIntervalsScenario PickScenario()
    {
        var idx = Random.Shared.Next(0, _scenarios.Count);
        return _scenarios[idx];
    }

    private void UpdateUi()
    {
        if (_t1OverrideText is not null)
        {
            Timer1RemainingText = _t1OverrideText;
        }
        else if (_isRunning)
        {
            var now = DateTimeOffset.UtcNow;
            var t1rem = GetRemainingSeconds(_t1DueUtc, now);
            Timer1RemainingText = $"{t1rem}s";
        }
        else
        {
            Timer1RemainingText = $"{Math.Max(0, _t1StoredRemainingSeconds)}s";
        }

        if (_t2OverrideText is not null)
        {
            Timer2RemainingText = _t2OverrideText;
        }
        else if (_cfg is null || !_cfg.Timer2Enabled)
        {
            Timer2RemainingText = "-";
        }
        else if (_isRunning)
        {
            var now = DateTimeOffset.UtcNow;
            var t2rem = GetRemainingSeconds(_t2DueUtc, now);
            Timer2RemainingText = $"{t2rem}s";
        }
        else
        {
            Timer2RemainingText = $"{Math.Max(0, _t2StoredRemainingSeconds)}s";
        }

        StartPauseText = _isRunning ? EndureIntervalsResources.Game_Pause : EndureIntervalsResources.Game_Start;
    }

    private void VibrateIfEnabled(TimeSpan duration)
    {
        if (_cfg?.VibrationsEnabled != true) return;
        try { Microsoft.Maui.Devices.Vibration.Default.Vibrate(duration); } catch { }
    }
}
