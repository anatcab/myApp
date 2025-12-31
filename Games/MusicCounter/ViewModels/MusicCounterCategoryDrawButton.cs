using CommunityToolkit.Mvvm.ComponentModel;

namespace fApp.Games.MusicCounter;

public partial class MusicCounterCategoryDrawButton : ObservableObject
{
    public string CategoryId { get; }
    public string DisplayName { get; }

    [ObservableProperty] private bool _isExhausted;

    public MusicCounterCategoryDrawButton(string categoryId, string displayName)
    {
        CategoryId = categoryId;
        DisplayName = displayName;
    }
}
