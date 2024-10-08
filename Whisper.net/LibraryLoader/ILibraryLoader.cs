// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.LibraryLoader;

internal interface ILibraryLoader
{
    LoadResult OpenLibrary(string? fileName);
}
