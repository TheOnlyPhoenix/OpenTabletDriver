using System;
using System.Runtime.InteropServices;

namespace OpenTabletDriver.Attributes
{
    /// <summary>
    /// Locks a class, struct, or interface to a specific platform.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class SupportedPlatformAttribute : Attribute
    {
        public SupportedPlatformAttribute(SystemPlatform platform)
        {
            this.Platform = platform;
        }

        public SystemPlatform Platform { get; }

        public bool IsCurrentPlatform =>
            (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Platform.HasFlag(SystemPlatform.Windows)) ||
            (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && Platform.HasFlag(SystemPlatform.Linux)) ||
            (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && Platform.HasFlag(SystemPlatform.MacOS)) ||
            (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD) && Platform.HasFlag(SystemPlatform.FreeBSD));
    }
}
