// © Microsoft Corporation. All rights reserved.

using System.Runtime.InteropServices;

namespace Whisper.net.Native.LibraryLoader;

internal class PosixLibraryLoader : ILibraryLoader
{
	[DllImport("libdl.so", ExactSpelling = true, CharSet = CharSet.Auto)]
	public static extern IntPtr dlopen(string filename, int flags);

	public IntPtr OpenLibrary(string filename, int flags)
	{
		try
		{
			return PosixLibraryLoader2.dlopen(filename, flags);
		}
		catch
		{
			return dlopen(filename, flags);
		}
	}

	private class PosixLibraryLoader2 : ILibraryLoader
	{
		[DllImport("libdl.so.2", ExactSpelling = true, CharSet = CharSet.Auto)]
		public static extern IntPtr dlopen(string filename, int flags);

		public IntPtr OpenLibrary(string filename, int flags)
		{
			return dlopen(filename, flags);
		}
	}
}