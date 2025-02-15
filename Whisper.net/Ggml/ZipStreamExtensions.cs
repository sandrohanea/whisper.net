// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.IO.Compression;

namespace Whisper.net.Ggml;

public static class ZipStreamExtensions
{
    /// <summary>
    /// Extracts the given zip stream to the given path.
    /// </summary>
    /// <param name="zipStream">The zip stream to be extracted.</param>
    /// <param name="path">The path.</param>
    /// <remarks>
    /// In order to work, you'll need to provide the same path as the ggml model.
    /// </remarks>
    /// <returns></returns>
    public static async Task ExtractToPath(this Task<Stream> zipStream, string path)
    {
        using var zipArchive = new ZipArchive(await zipStream, ZipArchiveMode.Read);
        zipArchive.ExtractToDirectory(path);
    }
}
