using System.Runtime.InteropServices;

namespace OpenTabletDriver.Interop
{
    public class SystemInterop
    {
        protected SystemInterop()
        {
        }

        public static SystemPlatform CurrentPlatform
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return SystemPlatform.Windows;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return SystemPlatform.Linux;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return SystemPlatform.MacOS;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                    return SystemPlatform.FreeBSD;

                return default;
            }
        }
    }
}
