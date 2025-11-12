using Microsoft.Extensions.Logging;

namespace Whisper.net.Tests.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp(Action<IServiceCollection>? services)
    {
        var builder = MauiApp.CreateBuilder();
        services?.Invoke(builder.Services);
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
