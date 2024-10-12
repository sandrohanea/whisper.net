// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Microsoft.Extensions.Logging;

namespace Whisper.net.Maui.Tests;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp(Action<IServiceCollection> configService)
    {
        var builder = MauiApp.CreateBuilder();
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

        configService(builder.Services);

        return builder.Build();
    }
}
