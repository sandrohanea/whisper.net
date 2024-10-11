// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Internals.Native;

namespace Whisper.net.LibraryLoader;

internal class LoadResult
{
    private LoadResult(bool isSuccess, string? errorMessage, INativeWhisper? nativeWhisper)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        NativeWhisper = nativeWhisper;
    }

    public static LoadResult Success(INativeWhisper nativeWhisper)
    {
        return new(true, null, nativeWhisper);
    }

    public static LoadResult Failure(string errorMessage)
    {
        return new(false, errorMessage, null);
    }

    public INativeWhisper? NativeWhisper;

    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
}
