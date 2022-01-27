using System;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Interop.Input.Absolute;
using OpenTabletDriver.Native.Linux.Evdev;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Binding.LinuxArtistMode
{
    [PluginName("Linux Artist Mode"), SupportedPlatform(SystemPlatform.Linux)]
    public class LinuxArtistModeButtonBinding : IStateBinding
    {
        private readonly EvdevVirtualTablet virtualTablet = (EvdevVirtualTablet)DesktopInterop.VirtualTablet;

        public static string[] ValidButtons { get; } = {
            "Pen Button 1",
            "Pen Button 2",
            "Pen Button 3"
        };

        [Property("Button"), PropertyValidated(nameof(ValidButtons))]
        public string Button { get; set; }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            SetState(true);
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            SetState(false);
        }

        private void SetState(bool state)
        {
            var eventCode = Button switch
            {
                "Pen Button 1" => EventCode.BTN_STYLUS,
                "Pen Button 2" => EventCode.BTN_STYLUS2,
                "Pen Button 3" => EventCode.BTN_STYLUS3,
                _ => throw new InvalidOperationException($"Invalid Button '{Button}'")
            };

            virtualTablet.SetKeyState(eventCode, state);
        }
    }
}
