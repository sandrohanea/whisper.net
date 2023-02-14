using System.Runtime.InteropServices;

namespace Whisper.net.Native.LibraryLoader;

internal class MacOsLibraryLoader : ILibraryLoader
{
	[DllImport("libdl.dylib", ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "dlopen")]
	public static extern IntPtr NativeOpenLibraryLibdl(string filename, int flags);

	public IntPtr OpenLibrary(string filename, int flags)
	{
		return NativeOpenLibraryLibdl(filename, flags);
	}
}