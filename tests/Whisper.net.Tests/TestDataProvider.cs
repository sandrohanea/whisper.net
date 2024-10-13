// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Tests;

public class TestDataProvider
{
    public static Func<string, Task<Stream>> OpenFileStreamAsync { get; set; } = DefaultOpenFileStreamAsync;

    private static Task<Stream> DefaultOpenFileStreamAsync(string file)
    {
        var fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        var streamReader = new StreamReader(fileStream);
        return Task.FromResult(streamReader.BaseStream);
    }
}
