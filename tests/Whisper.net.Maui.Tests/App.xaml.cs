// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Maui.Tests;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
