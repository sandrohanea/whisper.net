// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Whisper.net.Maui.Tests;

internal class Program : MauiApplication
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    static void Main(string[] args)
    {
        var app = new Program();
        app.Run(args);
    }
}
