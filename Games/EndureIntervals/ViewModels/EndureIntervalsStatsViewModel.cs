using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace fApp.Games.EndureIntervals;

public partial class EndureIntervalsStatsViewModel : ObservableObject
{
    [ObservableProperty] private bool _isTimer2Visible;

    [ObservableProperty] private string _timer1MaxRepeatsReachedText = "0";
    [ObservableProperty] private string _timer2SequenceCountText = "0";
    [ObservableProperty] private string _timer2DelayAfter2SumText = "0";
    [ObservableProperty] private string _timer2BaseAfter3SumText = "0";

    public void SetData(EndureIntervalsStatsSnapshot snap)
    {
        IsTimer2Visible = snap.Timer2Enabled;

        Timer1MaxRepeatsReachedText = snap.Timer1MaxRepeatsReached.ToString();

        Timer2SequenceCountText = snap.Timer2SequenceCount.ToString();
        Timer2DelayAfter2SumText = $"{snap.Timer2DelayAfter2SumSeconds}s";
        Timer2BaseAfter3SumText = $"{snap.Timer2BaseAfter3SumSeconds}s";
    }

    [RelayCommand]
    private Task Back()
    {
        return Application.Current!.MainPage!.Navigation.PopToRootAsync();
    }
}

public sealed record EndureIntervalsStatsSnapshot(
    int Timer1MaxRepeatsReached,
    bool Timer2Enabled,
    int Timer2SequenceCount,
    int Timer2DelayAfter2SumSeconds,
    int Timer2BaseAfter3SumSeconds
);
