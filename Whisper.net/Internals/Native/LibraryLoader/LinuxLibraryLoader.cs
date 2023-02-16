using System.Runtime.InteropServices;

namespace Whisper.net.Native.LibraryLoader;

internal class LinuxLibraryLoader : ILibraryLoader
{
	[DllImport("libdl.so", ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "dlopen")]
	public static extern IntPtr NativeOpenLibraryLibdl(string filename, int flags);

	[DllImport("libdl.so.2", ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "dlopen")]
	public static extern IntPtr NativeOpenLibraryLibdl2 (string filename, int flags);

	public IntPtr OpenLibrary(string filename, int flags)
	{
		try
		{
			return NativeOpenLibraryLibdl2(filename, flags);
		}
		catch (DllNotFoundException)
		{
			return NativeOpenLibraryLibdl(filename, flags);
		}
	}
}