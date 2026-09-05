// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Internals.Native;
using Whisper.net.LibraryLoader;

namespace Whisper.net.Internals;

internal static class ParakeetLibrary
{
    private static readonly Lazy<ParakeetLoadResult> LibraryLoaded =
        new(NativeLibraryLoader.LoadParakeetLibrary, true);

    public static INativeParakeet NativeParakeet
    {
        get
        {
            if (!LibraryLoaded.Value.IsSuccess)
            {
                throw new Exception(
                    $"Failed to load native Parakeet library. Error: {LibraryLoaded.Value.ErrorMessage}");
            }

            return LibraryLoaded.Value.NativeParakeet!;
        }
    }
}
