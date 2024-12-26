// Licensed under the MIT license: https://opensource.org/licenses/MIT
using UIKit;

namespace Whisper.net.Maui.Tests.Platforms.iOS;
public class Program
{
    // This is the main entry point of the application.
    static void Main(string[] args)
    {
        UIApplication.Main(args, null, typeof(TestApplicationDelegate));
    }
}
