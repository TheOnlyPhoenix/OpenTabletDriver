using System;
using System.Diagnostics;
using OpenTabletDriver.Desktop.Interop.Display;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Desktop.Interop.Input.Keyboard;
using OpenTabletDriver.Desktop.Interop.Input.Relative;
using OpenTabletDriver.Desktop.Interop.Timer;
using OpenTabletDriver.Desktop.Updater;
using OpenTabletDriver.Interop;
using OpenTabletDriver.Platform.Display;
using OpenTabletDriver.Platform.Keyboard;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Interop
{
    public class DesktopInterop : SystemInterop
    {
        protected DesktopInterop()
        {
        }

        private static IUpdater updater;
        private static IVirtualScreen virtualScreen;
        private static IAbsolutePointer absolutePointer;
        private static IRelativePointer relativePointer;
        private static IPressureHandler virtualTablet;
        private static IVirtualKeyboard virtualKeyboard;

        public static void Open(string path)
        {
            switch (CurrentPlatform)
            {
                case SystemPlatform.Windows:
                    var startInfo = new ProcessStartInfo("cmd", $"/c start {path.Replace("&", "^&")}")
                    {
                        CreateNoWindow = true
                    };
                    Process.Start(startInfo);
                    break;
                case SystemPlatform.Linux:
                    Process.Start("xdg-open", $"\"{path}\"");
                    break;
                case SystemPlatform.MacOS:
                case SystemPlatform.FreeBSD:
                    Process.Start("open", $"\"{path}\"");
                    break;
            }
        }

        public static void OpenFolder(string path)
        {
            switch (CurrentPlatform)
            {
                case SystemPlatform.Windows:
                    Process.Start("explorer", $"\"{path.Replace("&", "^&")}\"");
                    break;
                default:
                    Open(path);
                    break;
            }
        }

        public static IUpdater Updater => CurrentPlatform switch
        {
            SystemPlatform.Windows => updater ??= new WindowsUpdater(),
            SystemPlatform.MacOS => updater ??= new MacOSUpdater(),
            _ => null
        };

        public static ITimer Timer => CurrentPlatform switch
        {
            SystemPlatform.Windows => new WindowsTimer(),
            _ => new FallbackTimer()
        };

        public static IAbsolutePointer AbsolutePointer => CurrentPlatform switch
        {
            SystemPlatform.Windows => new WindowsAbsolutePointer(),
            SystemPlatform.Linux => absolutePointer ??= new EvdevAbsolutePointer(),
            SystemPlatform.MacOS => new MacOSAbsolutePointer(),
            _ => null
        };

        public static IRelativePointer RelativePointer => CurrentPlatform switch
        {
            SystemPlatform.Windows => new WindowsRelativePointer(),
            SystemPlatform.Linux => relativePointer ??= new EvdevRelativePointer(),
            SystemPlatform.MacOS => new MacOSRelativePointer(),
            _ => null
        };

        public static IPressureHandler VirtualTablet => CurrentPlatform switch
        {
            SystemPlatform.Linux => virtualTablet ??= new EvdevVirtualTablet(),
            _ => null
        };

        public static IVirtualKeyboard VirtualKeyboard => CurrentPlatform switch
        {
            SystemPlatform.Windows => new WindowsVirtualKeyboard(),
            SystemPlatform.Linux => virtualKeyboard ??= new EvdevVirtualKeyboard(),
            SystemPlatform.MacOS => new MacOSVirtualKeyboard(),
            _ => null
        };

        public static IVirtualScreen VirtualScreen => virtualScreen ??= CurrentPlatform switch
        {
            SystemPlatform.Windows => new WindowsDisplay(),
            SystemPlatform.Linux => ConstructLinuxDisplay(),
            SystemPlatform.MacOS => new MacOSDisplay(),
            _ => null
        };

        private static IVirtualScreen ConstructLinuxDisplay()
        {
            if (Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") != null)
                return new WaylandDisplay();
            else if (Environment.GetEnvironmentVariable("DISPLAY") != null)
                return new XScreen();

            Log.Write("Display", "Neither Wayland nor X11 were detected, defaulting to X11.", LogLevel.Warning);
            return new XScreen();
        }
    }
}
