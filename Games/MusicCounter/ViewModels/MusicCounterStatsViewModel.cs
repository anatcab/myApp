using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;

namespace fApp.Games.MusicCounter;

public partial class MusicCounterStatsViewModel : ObservableObject
{
    public IReadOnlyList<MusicCounterSegmentRow> Segments => _segments;
    private List<MusicCounterSegmentRow> _segments = new();

    public IReadOnlyList<MusicCounterPhraseTotalRow> PhraseTotals => _phraseTotals;
    private List<MusicCounterPhraseTotalRow> _phraseTotals = new();

    [ObservableProperty] private bool _isPhraseTotalsExpanded;
    [ObservableProperty] private string _phraseTotalsHeaderText = "";

    public void SetData(IReadOnlyList<int> segmentTotals, IReadOnlyList<MusicCounterPhraseEntry> phrases)
    {
        _segments = segmentTotals
            .Select((v, idx) => new MusicCounterSegmentRow(idx + 1, v))
            .ToList();

        _phraseTotals = phrases
            .OrderByDescending(p => p.TotalCount)
            .Select(p => new MusicCounterPhraseTotalRow(p.DisplayText, p.TotalCount))
            .ToList();

        IsPhraseTotalsExpanded = false;
        PhraseTotalsHeaderText = $"{GetResource("Stats_Phrases")} ▼";

        OnPropertyChanged(nameof(Segments));
        OnPropertyChanged(nameof(PhraseTotals));
    }

    [RelayCommand]
    private void TogglePhraseTotals()
    {
        IsPhraseTotalsExpanded = !IsPhraseTotalsExpanded;
        PhraseTotalsHeaderText = IsPhraseTotalsExpanded
            ? $"{GetResource("Stats_Phrases")} ▲"
            : $"{GetResource("Stats_Phrases")} ▼";
    }

    [RelayCommand]
    private Task Back()
    {
        return Application.Current!.MainPage!.Navigation.PopToRootAsync();
    }

    private static string GetResource(string key)
    {
        var s = MusicCounterResources.ResourceManager.GetString(key, CultureInfo.CurrentUICulture);
        return string.IsNullOrWhiteSpace(s) ? key : s!;
    }
}

public sealed record MusicCounterSegmentRow(int Index, int Total);
public sealed record MusicCounterPhraseTotalRow(string Phrase, int Total);
