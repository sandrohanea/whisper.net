// Licensed under the MIT license: https://opensource.org/licenses/MIT
using UIKit;
using Whisper.net.Maui.Tests.Platforms.iOS;

namespace Whisper.net.Maui.Tests;
public class Program
{
    // This is the main entry point of the application.
    static void Main(string[] args)
    {
        if (args?.Length > 0) // usually means this is from xharness
        {
            UIApplication.Main(args, null, typeof(TestApplicationDelegate));

        }
        else
        {
            UIApplication.Main(args, null, typeof(AppDelegate));
        }
    }
}
