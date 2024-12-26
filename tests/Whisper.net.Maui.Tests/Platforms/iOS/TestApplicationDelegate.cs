// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Globalization;
using Foundation;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.DotNet.XHarness.TestRunners.Xunit;
using UIKit;
using Whisper.net.Tests;

namespace Whisper.net.Maui.Tests.Platforms.iOS;

[Register(nameof(TestApplicationDelegate))]
public class TestApplicationDelegate : UIApplicationDelegate
{
    public override UIWindow? Window { get; set; }

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        Window = new UIWindow(UIScreen.MainScreen.Bounds)
        {
            RootViewController = new ViewController()
        };
        Window.MakeKeyAndVisible();

        return true;
    }

    class ViewController : UIViewController
    {
        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            var entryPoint = new TestsEntryPoint();

            await entryPoint.RunAsync();
        }
    }

    class TestsEntryPoint : iOSApplicationEntryPoint
    {
        protected override bool LogExcludedTests => true;

        protected override int? MaxParallelThreads => Environment.ProcessorCount;

        protected override IDevice Device { get; } = new TestDevice();

        protected override IEnumerable<TestAssemblyInfo> GetTestAssemblies()
        {
            yield return new TestAssemblyInfo(typeof(ModelFixture).Assembly, typeof(ModelFixture).Assembly.Location);
        }

        protected override void TerminateWithSuccess()
        {
            Console.WriteLine("Exiting test run with success");

            var s = new ObjCRuntime.Selector("terminateWithSuccess");
            UIApplication.SharedApplication.PerformSelector(s, UIApplication.SharedApplication, 0);
        }

        protected override TestRunner GetTestRunner(LogWriter logWriter)
        {
            var testRunner = base.GetTestRunner(logWriter);
            testRunner.SkipCategories(["SkipOnIos"]);
            return testRunner;
        }
    }
}
