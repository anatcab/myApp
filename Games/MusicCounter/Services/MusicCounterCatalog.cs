using System.Text.Json;

namespace fApp.Games.MusicCounter;

public sealed class MusicCounterCatalog : IMusicCounterCatalog
{
    private MusicCounterDefinition? _cached;

    public async Task<MusicCounterDefinition> GetAsync()
    {
        if (_cached is not null) return _cached;

        using var stream = await FileSystem.OpenAppPackageFileAsync("music_counter_catalog.json");
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();

        var dto = JsonSerializer.Deserialize<MusicCounterCatalogDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Invalid music_counter_catalog.json");

        _cached = new MusicCounterDefinition(
            dto.Categories.Select(c =>
                new MusicCounterCategoryDefinition(
                    c.Id,
                    c.NameKey,
                    c.Phrases.Select(p => new MusicCounterPhraseDefinition(p.Id, p.TextKey)).ToList()
                )
            ).ToList()
        );

        return _cached;
    }

    private sealed class MusicCounterCatalogDto
    {
        public List<CategoryDto> Categories { get; set; } = new();
    }

    private sealed class CategoryDto
    {
        public string Id { get; set; } = "";
        public string NameKey { get; set; } = "";
        public List<PhraseDto> Phrases { get; set; } = new();
    }

    private sealed class PhraseDto
    {
        public string Id { get; set; } = "";
        public string TextKey { get; set; } = "";
    }
}
