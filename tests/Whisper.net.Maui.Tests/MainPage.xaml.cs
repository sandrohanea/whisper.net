// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Ggml;

namespace Whisper.net.Maui.Tests;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.Tiny, QuantizationType.Q4_0);
            await modelStream.CopyToAsync(memoryStream);
            using var mauiStream = await FileSystem.OpenAppPackageFileAsync("kennedy.wav");
            var audioFileStream = new MemoryStream();
            // We need to copy it to a memory because the mauiStream cannot read Position.
            await mauiStream.CopyToAsync(audioFileStream);
            audioFileStream.Seek(0, SeekOrigin.Begin);

            using var whisperFactory = WhisperFactory.FromBuffer(memoryStream.GetBuffer().AsMemory(0, (int)memoryStream.Length));
            var whisperBuilder = whisperFactory.CreateBuilder();
            using var whisperProcessor = whisperBuilder.Build();
            LblResult.Text = string.Empty;
            await foreach (var result in whisperProcessor.ProcessAsync(audioFileStream))
            {
                LblResult.Text += result.Text;
            }
        }
        catch (Exception ex)
        {
            LblResult.Text = ex.Message;
            // ignored
        }
    }
}

