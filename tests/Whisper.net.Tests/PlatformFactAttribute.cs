// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Xunit;

namespace Whisper.net.Tests;
public class PlatformFactAttribute : FactAttribute
{
    public PlatformFactAttribute(string excludedPlatform)
    {
        var osDescription = RuntimeInformation.OSDescription;
#if NET8_0_OR_GREATER
        if (osDescription.Contains(excludedPlatform, StringComparison.OrdinalIgnoreCase))
        {
            Skip = $"Test skipped on {excludedPlatform}";
        }
#else
        if (osDescription.IndexOf(excludedPlatform, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            Skip = $"Test skipped on {excludedPlatform}";
        }
#endif
    }
}
