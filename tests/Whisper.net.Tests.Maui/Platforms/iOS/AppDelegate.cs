using Foundation;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.DotNet.XHarness.TestRunners.Xunit;
using Microsoft.Maui.LifecycleEvents;
using UIKit;
using Whisper.net.Tests.SpeechToText;

namespace Whisper.net.Tests.Maui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp(null, events =>
    {
        events.AddiOS(i =>
        {
            i.FinishedLaunching((app, options) =>
            {
                // Fire and forget on-device tests
                _ = RunAsync();
                return true;
            });
        });
    });


    private static async Task RunAsync()
    {
        try
        {
            await Task.Delay(1000);
            var entryPoint = new TestsEntryPoint();
            await entryPoint.RunAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // Exit the app
            Console.WriteLine(e);
        }
    }

    class TestsEntryPoint : iOSApplicationEntryPoint
    {
        protected override bool LogExcludedTests => true;

        protected override int? MaxParallelThreads => Environment.ProcessorCount;

        protected override IDevice Device { get; } = new TestDevice();

        protected override IEnumerable<TestAssemblyInfo> GetTestAssemblies()
        {
            yield return new TestAssemblyInfo(typeof(MauiTest).Assembly, typeof(MauiTest).Assembly.Location);
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
            return testRunner;
        }
    }
}
