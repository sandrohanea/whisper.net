// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Android.App;
using Android.Runtime;
using Whisper.net.Maui.Tests.Platforms.Android;

namespace Whisper.net.Maui.Tests;
[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp()
    {
        return MauiProgram.CreateMauiApp(services => services.AddTransient(svc => new HeadlessTestRunner("testresults.xml")));
    }
}
