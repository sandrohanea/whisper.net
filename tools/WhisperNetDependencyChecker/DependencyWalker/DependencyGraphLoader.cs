// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace WhisperNetDependencyChecker.DependencyWalker;
internal class DependencyGraphLoader
{
    private readonly INativeDependencyProvider depProvider;
    private readonly IKnownLibraryPathProvider knownPathProvider;

    public DependencyGraphLoader(INativeDependencyProvider depProvider, IKnownLibraryPathProvider knownPathProvider)
    {
        this.depProvider = depProvider;
        this.knownPathProvider = knownPathProvider;
    }

    /// <summary>
    /// Builds the dependency graph and returns a list of library names in load order.
    /// </summary>
    public List<string> BuildLoadOrder(string initialLibraryPath)
    {
        // Our graph maps a library name to its dependencies.
        var graph = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        // Use the file name (e.g. "A.dll") as the node identifier.
        var initialLibName = Path.GetFileName(initialLibraryPath);
        BuildGraphRecursive(initialLibName, initialLibraryPath, graph, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        return TopologicalSort(graph);
    }

    private void BuildGraphRecursive(string libName, string libPath, Dictionary<string, List<string>> graph, HashSet<string> visited)
    {
        if (visited.Contains(libName))
        {
            return;
        }

        visited.Add(libName);

        // Get dependencies from the provider and filter out known system libraries.
        var dependencies = depProvider
            .GetDependencies(libPath)
            .Where(dep => !ShouldSkipDependency(dep))
            .ToList();

        graph[libName] = dependencies;

        // Recurse only for non-system dependencies.
        foreach (var dep in dependencies)
        {
            var depPath = ResolveLibraryPath(dep, Path.GetDirectoryName(libPath));
            if (depPath != null)
            {
                BuildGraphRecursive(dep, depPath, graph, visited);
            }
            else
            {
                if (!graph.ContainsKey(dep))
                {
                    graph[dep] = [];
                }
            }
        }
    }

    /// <summary>
    /// Attempts to resolve the full path of a dependency by checking the base directory first, then known paths.
    /// </summary>
    public string? ResolveLibraryPath(string libraryName, string? baseDirectory)
    {
        // 1. Check the same folder as the initial native library.
        if (baseDirectory != null)
        {
            var candidate = Path.Combine(baseDirectory, libraryName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        // 2. Check each known path.
        foreach (var knownPath in knownPathProvider.GetKnownPaths())
        {
            var candidate = Path.Combine(knownPath, libraryName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        // Not found.
        return null;
    }

    /// <summary>
    /// Performs a topological sort on the dependency graph.
    /// </summary>
    private static List<string> TopologicalSort(Dictionary<string, List<string>> graph)
    {
        var sorted = new List<string>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var tempMarks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var node in graph.Keys)
        {
            if (!visited.Contains(node))
            {
                if (!Visit(node, graph, visited, tempMarks, sorted))
                {
                    throw new Exception("Cycle detected in dependency graph.");
                }
            }
        }
        return sorted;
    }

    private static bool Visit(string node, Dictionary<string, List<string>> graph,
                       HashSet<string> visited, HashSet<string> tempMarks, List<string> sorted)
    {
        if (tempMarks.Contains(node))
        {
            return false; // cycle detected
        }

        if (!visited.Contains(node))
        {
            tempMarks.Add(node);
            if (graph.TryGetValue(node, out var deps))
            {
                foreach (var dep in deps)
                {
                    if (!Visit(dep, graph, visited, tempMarks, sorted))
                    {
                        return false;
                    }
                }
            }
            tempMarks.Remove(node);
            visited.Add(node);
            sorted.Add(node);
        }
        return true;
    }

    /// <summary>
    /// Checks if the given dependency should be skipped.
    /// On Windows, we skip well-known system libraries such as kernel32.dll, ntdll.dll,
    /// and any library that begins with "api-ms-win-".
    /// </summary>
    private static bool ShouldSkipDependency(string libraryName)
    {
        if (OperatingSystem.IsWindows())
        {
            if (libraryName.StartsWith("api-ms-win-", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var systemLibs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "kernel32.dll",
                "ntdll.dll",
                "user32.dll",
                "gdi32.dll",
                "advapi32.dll",
                "shell32.dll"
            };

            if (systemLibs.Contains(libraryName))
            {
                return true;
            }
        }
        else if (OperatingSystem.IsMacOS())
        {
            var macSystemLibs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
             {
                "libSystem.B.dylib",
                "libc++.1.dylib",
                "Accelerate",
                "Foundation",
                "Metal",
                "MetalKit",
                "CoreFoundation",
                "libobjc.A.dylib"
            };

            if (macSystemLibs.Contains(libraryName))
            {
                return true;
            }
        }
        else
        {
            var linuxLibs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
             {
                "libdl.so.2",
                "libpthread.so.0"
            };

            if (linuxLibs.Contains(libraryName))
            {
                return true;
            }
        }
        return false;
    }
}
