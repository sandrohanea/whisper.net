// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Internals.Native.LibraryLoader;

internal class LoadResult
{
    private LoadResult(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public static LoadResult Success { get; } = new(true, null);

    public static LoadResult Failure(string errorMessage)
    {
        return new(false, errorMessage);
    }

    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
}
