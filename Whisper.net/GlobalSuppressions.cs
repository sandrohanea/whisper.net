// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Interoperability", "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time", Justification = "Not supported in netstandard, might need to use 2 versions", Scope = "member", Target = "~M:Whisper.net.Native.NativeMethods.whisper_init_state(System.IntPtr)~System.IntPtr")]
