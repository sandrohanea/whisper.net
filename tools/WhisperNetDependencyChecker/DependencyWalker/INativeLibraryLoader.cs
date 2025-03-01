namespace WhisperNetDependencyChecker.DependencyWalker;

internal interface INativeLibraryLoader
{

    bool TryOpenLibrary(string fileName, out IntPtr libHandle);

    string GetLastError();
    void CloseLibrary(IntPtr handle);
}
