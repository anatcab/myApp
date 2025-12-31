using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using fApp.Shared;
using System.Collections.ObjectModel;
using System.Globalization;

namespace fApp.Games.MusicCounter;

public partial class MusicCounterGameViewModel : ObservableObject
{
    private readonly IMusicCounterCatalog _catalog;
    private readonly INonRepeatingRandom _rng;

    private MusicCounterDefinition? _def;

    [ObservableProperty] private int _globalCount;
    [ObservableProperty] private int _resetCount;
    [ObservableProperty] private bool _canDrawAny;

    public ObservableCollection<MusicCounterPhraseEntry> Phrases { get; } = new();
    public ObservableCollection<MusicCounterCategoryDrawButton> CategoryDrawButtons { get; } = new();

    private readonly Dictionary<string, List<MusicCounterPhraseDefinition>> _remainingByCategory = new();
    private readonly Dictionary<string, MusicCounterPhraseEntry> _phraseIndex = new();

    private readonly List<int> _segmentTotals = new();
    private int _currentSegmentTotal;

    public MusicCounterGameViewModel(IMusicCounterCatalog catalog, INonRepeatingRandom rng)
    {
        _catalog = catalog;
        _rng = rng;
    }

    public async Task InitializeAsync(IReadOnlyList<(string CategoryId, int InitialDraws)> selected)
    {
        _def = await _catalog.GetAsync();

        GlobalCount = 0;
        ResetCount = 0;
        CanDrawAny = false;

        _segmentTotals.Clear();
        _currentSegmentTotal = 0;

        Phrases.Clear();
        CategoryDrawButtons.Clear();
        _remainingByCategory.Clear();
        _phraseIndex.Clear();

        var selectedIds = selected.Select(s => s.CategoryId).ToHashSet();

        foreach (var cat in _def.Categories.Where(c => selectedIds.Contains(c.Id)))
        {
            _remainingByCategory[cat.Id] = cat.Phrases.ToList();
            CategoryDrawButtons.Add(new MusicCounterCategoryDrawButton(cat.Id, GetResource(cat.NameKey)));
        }

        foreach (var (categoryId, initialDraws) in selected)
        {
            for (var i = 0; i < initialDraws; i++)
                TryDrawFromCategory(categoryId);
        }

        UpdateExhaustedFlags();
    }

    [RelayCommand]
    private void End()
    {
        _segmentTotals.Add(_currentSegmentTotal);

        var stats = Application.Current!.Handler!.MauiContext!.Services.GetRequiredService<MusicCounterStatsPage>();
        var vm = (MusicCounterStatsViewModel)stats.BindingContext;
        vm.SetData(_segmentTotals, Phrases.ToList());

        Application.Current!.MainPage!.Navigation.PushAsync(stats);
    }

    [RelayCommand]
    private void Reset()
    {
        _segmentTotals.Add(_currentSegmentTotal);
        _currentSegmentTotal = 0;

        ResetCount += 1;
        GlobalCount = 0;

        foreach (var p in Phrases)
            p.CurrentCount = 0;
    }

    [RelayCommand]
    private void DrawAny()
    {
        if (!CanDrawAny)
        {
            UpdateExhaustedFlags();
            return;
        }

        var candidates = CategoryDrawButtons
            .Where(b => !b.IsExhausted)
            .Select(b => b.CategoryId)
            .ToList();

        if (candidates.Count == 0)
        {
            UpdateExhaustedFlags();
            return;
        }

        if (!_rng.TryTake(candidates, out var pickedCategoryId) || pickedCategoryId is null)
        {
            UpdateExhaustedFlags();
            return;
        }

        TryDrawFromCategory(pickedCategoryId);
        UpdateExhaustedFlags();
    }

    [RelayCommand]
    private void DrawFromCategory(MusicCounterCategoryDrawButton button)
    {
        if (button.IsExhausted) return;
        TryDrawFromCategory(button.CategoryId);
        UpdateExhaustedFlags();
    }

    [RelayCommand]
    private void CountPhrase(MusicCounterPhraseEntry entry)
    {
        GlobalCount += 1;
        _currentSegmentTotal += 1;

        entry.CurrentCount += 1;
        entry.TotalCount += 1;
    }

    private void TryDrawFromCategory(string categoryId)
    {
        if (!_remainingByCategory.TryGetValue(categoryId, out var pool)) return;

        if (!_rng.TryTake(pool, out var phrase) || phrase is null) return;

        if (_phraseIndex.ContainsKey(phrase.Id)) return;

        var entry = new MusicCounterPhraseEntry(phrase.Id, GetResource(phrase.TextKey));
        _phraseIndex[phrase.Id] = entry;
        Phrases.Add(entry);
    }

    private void UpdateExhaustedFlags()
    {
        var any = false;

        foreach (var b in CategoryDrawButtons)
        {
            var exhausted = !_remainingByCategory.TryGetValue(b.CategoryId, out var pool) || pool.Count == 0;
            b.IsExhausted = exhausted;
            if (!exhausted) any = true;
        }

        CanDrawAny = any;
    }

    private static string GetResource(string key)
    {
        var s = MusicCounterResources.ResourceManager.GetString(key, CultureInfo.CurrentUICulture);
        return string.IsNullOrWhiteSpace(s) ? key : s!;
    }
}
