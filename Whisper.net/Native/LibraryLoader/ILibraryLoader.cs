// © Microsoft Corporation. All rights reserved.

namespace Whisper.net.Native.LibraryLoader;

internal interface ILibraryLoader
{
    IntPtr OpenLibrary(string filename, int flags);
}