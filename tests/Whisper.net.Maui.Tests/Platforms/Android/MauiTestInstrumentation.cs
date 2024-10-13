// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Whisper.net.Tests;
using Application = Android.App.Application;
using Environment = Android.OS.Environment;
namespace Whisper.net.Maui.Tests.Platforms.Android;

[Instrumentation(Name = "com.companyname.whisper.net.maui.tests.AndroidMauiTestInstrumentation")]
public class MauiTestInstrumentation : Instrumentation
{
    public IServiceProvider Services { get; private set; } = null!;
    readonly TaskCompletionSource<Application> _waitForApplication = new();

    public override void CallApplicationOnCreate(Application? app)
    {
        base.CallApplicationOnCreate(app);

        if (app == null)
        {
            _waitForApplication.SetException(new ArgumentNullException(nameof(app)));
        }
        else
        {
            _waitForApplication.SetResult(app);
        }
    }

    public MauiTestInstrumentation(IntPtr handle, JniHandleOwnership transfer)
        : base(handle, transfer)
    {

    }

    public override void OnCreate(Bundle? arguments)
    {
        base.OnCreate(arguments);

        Start();
    }

    private static async Task<Stream> OpenFileNameAsync(string file)
    {
        using var androidStream = await FileSystem.OpenAppPackageFileAsync(file);
        var memoryStream = new MemoryStream();
        // We need to copy it to a memory because the AndroidStream cannot read Position.
        await androidStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }

    public override async void OnStart()
    {
        TestDataProvider.OpenFileStreamAsync = OpenFileNameAsync;
        base.OnStart();

        await _waitForApplication.Task;

        Services = IPlatformApplication.Current?.Services ?? Services;

        var bundle = await RunTestsAsync();

        CopyFile(bundle);

        Finish(Result.Ok, bundle);
    }

    Task<Bundle> RunTestsAsync()
    {
        var runner = Services.GetRequiredService<HeadlessTestRunner>();

        return runner.RunTestsAsync();

    }

    private async void CopyFile(Bundle bundle)
    {
        var resultsFile = bundle.GetString("test-results-path");
        if (resultsFile == null)
        {
            return;
        }

        var guid = Guid.NewGuid().ToString("N");
        var name = Path.GetFileName(resultsFile);

        string finalPath;
        if ((int)Build.VERSION.SdkInt < 30)

        {
            var root = Application.Context.GetExternalFilesDir(null)!.AbsolutePath!;
            var dir = Path.Combine(root, guid);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            finalPath = Path.Combine(dir, name);
            File.Copy(resultsFile, finalPath, true);
        }

        else
        {
            
            var downloads = Environment.DirectoryDownloads!;
            var relative = Path.Combine(downloads, "com.companyname.whisper.net.maui.tests", guid);

            var values = new ContentValues();
            values.Put(MediaStore.IMediaColumns.DisplayName, name);
            values.Put(MediaStore.IMediaColumns.MimeType, "text/xml");
            values.Put(MediaStore.IMediaColumns.RelativePath, relative);
            
            var resolver = Context!.ContentResolver!;
            var uri = resolver.Insert(MediaStore.Downloads.ExternalContentUri, values)!;
            using (var dest = resolver.OpenOutputStream(uri)!)
            using (var source = File.OpenRead(resultsFile))
            {
                await source.CopyToAsync(dest);
            }

            var root = Environment.ExternalStorageDirectory!.AbsolutePath;
            finalPath = Path.Combine(root, relative, name);
        }

        bundle.PutString("test-results-path", finalPath);
    }
}
