using Microsoft.Extensions.Logging;

namespace fApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Allura-Regular.ttf", "Allura");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<IGameCatalog, GameCatalog>();

        builder.Services.AddSingleton<GamesListViewModel>();
        builder.Services.AddSingleton<GamesListPage>();

        builder.Services.AddSingleton<fApp.Shared.INonRepeatingRandom, fApp.Shared.NonRepeatingRandom>();

        builder.Services.AddSingleton<fApp.Games.MusicCounter.IMusicCounterCatalog, fApp.Games.MusicCounter.MusicCounterCatalog>();

        builder.Services.AddTransient<fApp.Games.MusicCounter.MusicCounterSetupViewModel>();
        builder.Services.AddTransient<fApp.Games.MusicCounter.MusicCounterGameViewModel>();
        builder.Services.AddTransient<fApp.Games.MusicCounter.MusicCounterStatsViewModel>();

        builder.Services.AddTransient<fApp.Games.MusicCounter.MusicCounterSetupPage>();
        builder.Services.AddTransient<fApp.Games.MusicCounter.MusicCounterGamePage>();
        builder.Services.AddTransient<fApp.Games.MusicCounter.MusicCounterStatsPage>();

        return builder.Build();
    }
}
