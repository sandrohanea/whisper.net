// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Internals.Native.Implementations;
internal static class NativeConstants
{
    private const string libPrefix = "lib";

    public const string WhisperLibraryName = "whisper";
    public const string GgmlWhisperLibraryName = "ggml-whisper";
    public const string LibWhisperLibraryName = libPrefix + WhisperLibraryName;
    public const string LibGgmlWhisperLibraryName = libPrefix + GgmlWhisperLibraryName;

    public const string InternalLibraryName = "__Internal";
}
