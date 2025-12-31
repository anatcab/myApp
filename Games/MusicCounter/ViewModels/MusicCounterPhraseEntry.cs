using CommunityToolkit.Mvvm.ComponentModel;

namespace fApp.Games.MusicCounter;

public partial class MusicCounterPhraseEntry : ObservableObject
{
    public string PhraseId { get; }
    public string DisplayText { get; }

    [ObservableProperty] private int _currentCount;
    [ObservableProperty] private int _totalCount;

    public MusicCounterPhraseEntry(string phraseId, string displayText)
    {
        PhraseId = phraseId;
        DisplayText = displayText;
    }
}
