// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Internals.Native;

namespace Whisper.net.LibraryLoader;

internal sealed class ParakeetLoadResult
{
    private ParakeetLoadResult(bool isSuccess, string? errorMessage, INativeParakeet? nativeParakeet)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        NativeParakeet = nativeParakeet;
    }

    public static ParakeetLoadResult Success(INativeParakeet nativeParakeet)
    {
        return new(true, null, nativeParakeet);
    }

    public static ParakeetLoadResult Failure(string errorMessage)
    {
        return new(false, errorMessage, null);
    }

    public INativeParakeet? NativeParakeet { get; }
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
}
