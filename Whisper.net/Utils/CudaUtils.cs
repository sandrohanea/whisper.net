using ManagedCuda;

namespace Whisper.net.Utils;

internal static class CudaUtils
{
    public static bool IsCudaAvailable()
    {
        try
        {
            var deviceCount = CudaContext.GetDeviceCount();
            return deviceCount > 0;
        }
        catch
        {
            return false;
        }
    }

    public static int GetCudaCores()
    {
        try
        {
            var count = 0;
            var deviceCount = CudaContext.GetDeviceCount();
            for (var i = 0; i < deviceCount; i++)
            {
                var deviceProperties = CudaContext.GetDeviceInfo(i);
                var coresPerSm = ConvertCoresPerSm(deviceProperties.ComputeCapability.Major, deviceProperties.ComputeCapability.Minor);
                count += deviceProperties.MultiProcessorCount * coresPerSm;
            }

            return count;
        }
        catch
        {
            return -1;
        }

        int ConvertCoresPerSm(int major, int minor)
        {
            switch (major)
            {
                case 2:
                    return minor switch
                    {
                        1 => 48,
                        _ => 32
                    };
                case 3:
                    return 192;
                case 5:
                    return 128;
                case 6:
                    switch (minor)
                    {
                        case 1:
                        case 2:
                            return 128;
                        case 0:
                            return 64;
                    }
                    break;
                case 7:
                    if (minor is 0 or 5)
                    {
                        return 64;
                    }
                    break;
                case 8:
                    switch (minor)
                    {
                        case 0:
                            return 64;
                        case 6:
                        case 9:
                            return 128;
                    }
                    break;
                case 9:
                    if (minor == 0)
                    {
                        return 128;
                    }
                    break;
            }

            return -1;
        }
    }
}
