// © Microsoft Corporation. All rights reserved.

using System.Runtime.InteropServices;

namespace Whisper.net.Native.LibraryLoader;

internal class PosixLibraryLoader : ILibraryLoader
{
	[DllImport("libdl.so", ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "dlopen")]
	public static extern IntPtr OpenLibdl(string filename, int flags);

	[DllImport("libdl.so.2", ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "dlopen")]
	public static extern IntPtr OpenLibdl2 (string filename, int flags);

	public IntPtr OpenLibrary(string filename, int flags)
	{
		try
		{
			return OpenLibdl(filename, flags);
		}
		catch
		{
			return OpenLibdl2(filename, flags);
		}
	}
}