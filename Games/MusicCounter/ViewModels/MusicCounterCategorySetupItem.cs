using CommunityToolkit.Mvvm.ComponentModel;

namespace fApp.Games.MusicCounter;

public partial class MusicCounterCategorySetupItem : ObservableObject
{
    public string CategoryId { get; }
    public string DisplayName { get; }
    public int MaxDrawCount { get; }

    [ObservableProperty] private int _drawCount;
    [ObservableProperty] private bool _isSelected;

    public MusicCounterCategorySetupItem(string categoryId, string displayName, int maxDrawCount)
    {
        CategoryId = categoryId;
        DisplayName = displayName;
        MaxDrawCount = maxDrawCount;
    }
}
