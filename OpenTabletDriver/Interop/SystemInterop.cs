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
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return SystemPlatform.Linux;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return SystemPlatform.MacOS;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                    return SystemPlatform.FreeBSD;
                else
                    return 0;
            }
        }
    }
}
