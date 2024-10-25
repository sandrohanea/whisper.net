// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.LibraryLoader;

internal interface ILibraryLoader
{
    bool TryOpenLibrary(string fileName, out IntPtr libHandle);

    string GetLastError();
}
