using Android.App;
using Android.Runtime;
using Whisper.net.Tests.Maui.Platforms.Android;

namespace Whisper.net.Tests.Maui;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp(services => services.AddTransient(svc => new HeadlessTestRunner("testresults.xml")));

}
