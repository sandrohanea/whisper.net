// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Internals.Native;
using Whisper.net.LibraryLoader;
using Whisper.net.Logger;

namespace Whisper.net.Internals;

internal static class WhisperLibrary
{
    private static readonly Lazy<LoadResult> LibraryLoaded = new(() =>
    {
        var localLibraryLoaded = NativeLibraryLoader.LoadNativeLibrary();
        if (localLibraryLoaded.IsSuccess)
        {
            LogProvider.InitializeLogging(localLibraryLoaded.NativeWhisper!);
        }

        return localLibraryLoaded;
    }, true);

    public static INativeWhisper NativeWhisper
    {
        get
        {
            CheckLibraryLoaded();
            return LibraryLoaded.Value.NativeWhisper!;
        }
    }

    private static void CheckLibraryLoaded()
    {
        if (!LibraryLoaded.Value.IsSuccess)
        {
            throw new Exception($"Failed to load native whisper library. Error: {LibraryLoaded.Value.ErrorMessage}");
        }
    }
}
