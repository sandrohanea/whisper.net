// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.LibraryLoader;

internal interface ILibraryLoader
{
    IntPtr OpenLibrary(string fileName);

    string GetLastError();
}
