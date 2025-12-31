namespace fApp.Games.MusicCounter;

public interface IMusicCounterCatalog
{
    Task<MusicCounterDefinition> GetAsync();
}
