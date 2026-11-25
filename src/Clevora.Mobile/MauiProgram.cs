using Microsoft.Extensions.Logging;
using Clevora.Mobile.Services;

namespace Clevora.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddHttpClient();

        // Local SQLite database (singleton — one connection per app lifetime)
        builder.Services.AddSingleton<LocalDatabaseService>();

        // Sync orchestrator
        builder.Services.AddSingleton<MobileSyncOrchestrator>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
