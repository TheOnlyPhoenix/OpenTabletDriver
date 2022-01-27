using System.Collections.Generic;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.DependencyInjection;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Desktop.Interop.Input.Keyboard;
using OpenTabletDriver.Platform.Keyboard;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class KeyBinding : IStateBinding
    {
        private const string PLUGIN_NAME = "Key Binding";

        [Resolved]
        public IVirtualKeyboard Keyboard { set; get; }

        [Property("Key"), PropertyValidated(nameof(ValidKeys))]
        public string Key { set; get; }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (!string.IsNullOrWhiteSpace(Key))
                Keyboard.Press(Key);
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (!string.IsNullOrWhiteSpace(Key))
                Keyboard.Release(Key);
        }

        private static IEnumerable<string> validKeys;
        public static IEnumerable<string> ValidKeys
        {
            get => validKeys ??= DesktopInterop.CurrentPlatform switch
            {
                SystemPlatform.Windows => WindowsVirtualKeyboard.EtoKeysymToVK.Keys,
                SystemPlatform.Linux => EvdevVirtualKeyboard.EtoKeysymToEventCode.Keys,
                SystemPlatform.MacOS => MacOSVirtualKeyboard.EtoKeysymToVK.Keys,
                _ => null
            };
        }

        public override string ToString() => $"{PLUGIN_NAME}: {Key}";
    }
}
