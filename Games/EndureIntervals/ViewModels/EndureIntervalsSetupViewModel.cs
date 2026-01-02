using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using fApp.Games.EndureIntervals;
using fApp.Shared;

namespace fApp.Games.EndureIntervals;

public partial class EndureIntervalsSetupViewModel : ObservableObject
{
    private readonly IRandomTimeProvider _rng;

    public EndureIntervalsSetupViewModel(IRandomTimeProvider rng)
    {
        _rng = rng;

        Timer1MinSeconds = 20;
        Timer1MaxSeconds = 60;
        Timer1MaxRepeats = 12;

        Timer2Enabled = true;
        Timer2MinSeconds = 60;
        Timer2MaxSeconds = 180;

        RecalcCanStart();
    }

    [ObservableProperty] private int _timer1MinSeconds;
    [ObservableProperty] private int _timer1MaxSeconds;
    [ObservableProperty] private int _timer1MaxRepeats;

    [ObservableProperty] private bool _timer2Enabled;
    [ObservableProperty] private int _timer2MinSeconds;
    [ObservableProperty] private int _timer2MaxSeconds;

    [ObservableProperty] private bool _canStart;

    public string Timer2EnabledLabel => Timer2Enabled ? "On" : "Off";

    partial void OnTimer1MinSecondsChanged(int value) => RecalcCanStart();
    partial void OnTimer1MaxSecondsChanged(int value) => RecalcCanStart();
    partial void OnTimer1MaxRepeatsChanged(int value) => RecalcCanStart();
    partial void OnTimer2EnabledChanged(bool value)
    {
        OnPropertyChanged(nameof(Timer2EnabledLabel));
        RecalcCanStart();
    }
    partial void OnTimer2MinSecondsChanged(int value) => RecalcCanStart();
    partial void OnTimer2MaxSecondsChanged(int value) => RecalcCanStart();

    private static int ClampStep(int v, int min, int max, int step)
    {
        v = Math.Clamp(v, min, max);
        var snapped = ((v - min) / step) * step + min;
        return Math.Clamp(snapped, min, max);
    }

    [RelayCommand] private void Timer1MinMinus() => Timer1MinSeconds = ClampStep(Timer1MinSeconds - 5, 5, 60, 5);
    [RelayCommand] private void Timer1MinPlus() => Timer1MinSeconds = ClampStep(Timer1MinSeconds + 5, 5, 60, 5);

    [RelayCommand] private void Timer1MaxMinus() => Timer1MaxSeconds = ClampStep(Timer1MaxSeconds - 10, 10, 360, 10);
    [RelayCommand] private void Timer1MaxPlus() => Timer1MaxSeconds = ClampStep(Timer1MaxSeconds + 10, 10, 360, 10);

    [RelayCommand] private void Timer1RepeatsMinus() => Timer1MaxRepeats = ClampStep(Timer1MaxRepeats - 1, 1, 15, 1);
    [RelayCommand] private void Timer1RepeatsPlus() => Timer1MaxRepeats = ClampStep(Timer1MaxRepeats + 1, 1, 15, 1);

    [RelayCommand] private void ToggleTimer2() => Timer2Enabled = !Timer2Enabled;

    [RelayCommand] private void Timer2MinMinus() => Timer2MinSeconds = ClampStep(Timer2MinSeconds - 10, 30, 120, 10);
    [RelayCommand] private void Timer2MinPlus() => Timer2MinSeconds = ClampStep(Timer2MinSeconds + 10, 30, 120, 10);

    [RelayCommand] private void Timer2MaxMinus() => Timer2MaxSeconds = ClampStep(Timer2MaxSeconds - 20, 60, 360, 20);
    [RelayCommand] private void Timer2MaxPlus() => Timer2MaxSeconds = ClampStep(Timer2MaxSeconds + 20, 60, 360, 20);

    [RelayCommand]
    private async Task Go()
    {
        if (!CanStart) return;

        var cfg = new EndureIntervalsConfig(
            Timer1Range: new TimeRange(Timer1MinSeconds, Timer1MaxSeconds),
            Timer1MaxRepeats: Timer1MaxRepeats,
            Timer2Enabled: Timer2Enabled,
            Timer2Range: new TimeRange(Timer2MinSeconds, Timer2MaxSeconds)
        );

        var game = Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<EndureIntervalsGamePage>();
        var vm = (EndureIntervalsGameViewModel)game.BindingContext;
        vm.Initialize(cfg);

        await Application.Current!.MainPage!.Navigation.PushAsync(game);
    }

    private void RecalcCanStart()
    {
        var t1ok = Timer1MinSeconds <= Timer1MaxSeconds;
        var t2ok = !Timer2Enabled || Timer2MinSeconds <= Timer2MaxSeconds;
        CanStart = t1ok && t2ok;
    }
}
