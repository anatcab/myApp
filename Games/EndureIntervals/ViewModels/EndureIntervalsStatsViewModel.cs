using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace fApp.Games.EndureIntervals;

public partial class EndureIntervalsStatsViewModel : ObservableObject
{
    [ObservableProperty] private string _timer1MaxDurationText = "";
    [ObservableProperty] private string _timer1MaxDurationCountText = "";
    [ObservableProperty] private string _timer2SequenceCountText = "";
    [ObservableProperty] private string _timer2DelayAfter2SumText = "";
    [ObservableProperty] private string _timer2BaseAfter3SumText = "";

    public void SetData(EndureIntervalsStatsSnapshot snap)
    {
        Timer1MaxDurationText = $"Timer1 max duration: {snap.Timer1MaxDurationSeconds}s";
        Timer1MaxDurationCountText = $"Timer1 max duration count: {snap.Timer1MaxDurationCount}";
        Timer2SequenceCountText = $"Timer2 sequences: {snap.Timer2SequenceCount}";
        Timer2DelayAfter2SumText = $"Timer2 sum delay-after-2: {snap.Timer2DelayAfter2SumSeconds}s";
        Timer2BaseAfter3SumText = $"Timer2 sum base-after-3: {snap.Timer2BaseAfter3SumSeconds}s";
    }

    [RelayCommand]
    private Task Back()
    {
        return Application.Current!.MainPage!.Navigation.PopToRootAsync();
    }
}

public sealed record EndureIntervalsStatsSnapshot(
    int Timer1MaxDurationSeconds,
    int Timer1MaxDurationCount,
    int Timer2SequenceCount,
    int Timer2DelayAfter2SumSeconds,
    int Timer2BaseAfter3SumSeconds
);
