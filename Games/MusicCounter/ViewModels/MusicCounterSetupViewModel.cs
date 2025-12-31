using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;
using System.Reflection.Metadata;

namespace fApp.Games.MusicCounter;

public partial class MusicCounterSetupViewModel : ObservableObject
{
    private readonly IMusicCounterCatalog _catalog;

    public IReadOnlyList<MusicCounterCategorySetupItem> Categories => _categories;
    private List<MusicCounterCategorySetupItem> _categories = new();

    [ObservableProperty] private bool _canStart;

    public MusicCounterSetupViewModel(IMusicCounterCatalog catalog)
    {
        _catalog = catalog;
    }

    [RelayCommand]
    private async Task Load()
    {
        if (_categories.Count > 0) return;

        var def = await _catalog.GetAsync();
        _categories = def.Categories
            .Select(c => new MusicCounterCategorySetupItem(
                c.Id,
                GetResource(c.NameKey),
                c.Phrases.Count
            ))
            .ToList();

        OnPropertyChanged(nameof(Categories));
        RecalcCanStart();
    }

    [RelayCommand]
    private void ToggleSelected(MusicCounterCategorySetupItem item)
    {
        item.IsSelected = !item.IsSelected;
        RecalcCanStart();
    }

    [RelayCommand]
    private void Increment(MusicCounterCategorySetupItem item)
    {
        if (item.DrawCount >= item.MaxDrawCount) return;
        item.DrawCount += 1;
    }

    [RelayCommand]
    private void Decrement(MusicCounterCategorySetupItem item)
    {
        if (item.DrawCount <= 0) return;
        item.DrawCount -= 1;
    }

    [RelayCommand]
    private async Task Go()
    {
        if (!CanStart) return;

        var selected = _categories
            .Where(c => c.IsSelected)
            .Select(c => (c.CategoryId, c.DrawCount))
            .ToList();

        if (selected.Count == 0) return;

        var game = Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<MusicCounterGamePage>();
        var vm = (MusicCounterGameViewModel)game.BindingContext;
        await vm.InitializeAsync(selected);

        await Application.Current!.MainPage!.Navigation.PushAsync(game);
    }

    private void RecalcCanStart()
    {
        CanStart = _categories.Any(c => c.IsSelected);
    }

    private static string GetResource(string key)
    {
        var s = MusicCounterResources.ResourceManager.GetString(key, CultureInfo.CurrentUICulture);
        return string.IsNullOrWhiteSpace(s) ? key : s!;
    }
}
