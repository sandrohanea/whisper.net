// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Native.LibraryLoader;

internal interface ILibraryLoader
{
    IntPtr OpenLibrary(string filename, int flags);
}