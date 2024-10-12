// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Foundation;

namespace Whisper.net.Maui.Tests;
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp()
    {
        return MauiProgram.CreateMauiApp(service => { });
    }
}
