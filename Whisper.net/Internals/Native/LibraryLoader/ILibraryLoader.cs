// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Internals.Native.LibraryLoader;

namespace Whisper.net.Native.LibraryLoader;

internal interface ILibraryLoader
{
    LoadResult OpenLibrary(string filename);
}
