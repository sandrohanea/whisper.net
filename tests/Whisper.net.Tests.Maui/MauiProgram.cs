using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace Whisper.net.Tests.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp(Action<IServiceCollection>? services = null, Action<ILifecycleBuilder>? configure = null)
    {
        var builder = MauiApp.CreateBuilder();
        services?.Invoke(builder.Services);
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureLifecycleEvents(events =>
            {
                configure?.Invoke(events);
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
