using ObjCRuntime;
using UIKit;
using Whisper.net.Tests.Maui.Platforms.iOS;

namespace Whisper.net.Tests.Maui;

public class Program
{
    // This is the main entry point of the application.
    static void Main(string[] args)
    {
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(TestApplicationDelegate));
    }
}
