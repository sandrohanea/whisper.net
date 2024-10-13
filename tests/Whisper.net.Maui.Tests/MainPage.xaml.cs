// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Internals.Native.Implementations;
using Whisper.net.LibraryLoader;
using Whisper.net.Logger;

namespace Whisper.net.Maui.Tests;

public partial class MainPage : ContentPage
{
    private int count;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        CounterBtn.Text = $"Clicked {count} times";

        try
        {
            var linuxLIbraryLoader = new LinuxLibraryLoader();
            var openedLib = linuxLIbraryLoader.OpenLibrary("libggml.so", true);

            if (openedLib == IntPtr.Zero)
            {
                var error = linuxLIbraryLoader.GetLastError();
                CounterBtn.Text = error;
            }

        }
        catch (Exception ex)
        {
            CounterBtn.Text = ex.Message;
            // ignored
        }

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}

