using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.Interop.Display;
using OpenTabletDriver.Desktop.Interop.Environment;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Keyboard;
using OpenTabletDriver.Desktop.Interop.Input.Relative;
using OpenTabletDriver.Desktop.Interop.Timer;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Keyboard;
using OpenTabletDriver.Platform.Pointer;

#nullable enable

namespace OpenTabletDriver.Desktop.Interop
{
    using static System.Environment;
    using static ServiceDescriptor;

    public sealed class DesktopLinuxServiceCollection : DesktopServiceCollection
    {
        private static readonly IEnumerable<ServiceDescriptor> PlatformRequiredServices = new[]
        {
            Singleton<IAppInfo, LinuxAppInfo>(),
            Transient<EnvironmentHandler, LinuxEnvironmentHandler>(),
            Transient<EnvironmentDictionary, LinuxEnvironmentDictionary>(),
            Transient<ITimer, FallbackTimer>(),
            Singleton<IAbsolutePointer, EvdevAbsolutePointer>(),
            Singleton<IRelativePointer, EvdevRelativePointer>(),
            Singleton<IPressureHandler, EvdevVirtualTablet>(),
            Singleton<IVirtualKeyboard, EvdevVirtualKeyboard>(),
            Singleton<IKeysProvider, LinuxKeysProvider>(),
            GetVirtualScreen()
        };

        public DesktopLinuxServiceCollection() : base(PlatformRequiredServices)
        {
        }

        private static ServiceDescriptor GetVirtualScreen()
        {
            if (GetEnvironmentVariable("WAYLAND_DISPLAY") != null)
                return Singleton<IVirtualScreen, WaylandDisplay>();
            if (GetEnvironmentVariable("DISPLAY") != null)
                return Singleton<IVirtualScreen, XScreen>();

            Log.Write("Display", "Neither Wayland nor X11 were detected, defaulting to X11.", LogLevel.Warning);
            return Singleton<IVirtualScreen, XScreen>();
        }
    }
}
