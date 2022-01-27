using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.DependencyInjection;
using OpenTabletDriver.Platform.Pointer;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class MouseBinding : IStateBinding
    {
        private const string PLUGIN_NAME = "Mouse Button Binding";

        [Resolved]
        public IMouseButtonHandler Pointer { set; get; }

        [Property("Button"), PropertyValidated(nameof(ValidButtons))]
        public string Button { set; get; }

        public void Press(TabletReference tablet, IDeviceReport report)
        {
            if (Enum.TryParse<MouseButton>(Button, true, out var mouseButton))
                Pointer?.MouseDown(mouseButton);
        }

        public void Release(TabletReference tablet, IDeviceReport report)
        {
            if (Enum.TryParse<MouseButton>(Button, true, out var mouseButton))
                Pointer?.MouseUp(mouseButton);
        }

        private static IEnumerable<string> validButtons;
        public static IEnumerable<string> ValidButtons
        {
            get => validButtons ??= Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>().Select(Enum.GetName);
        }

        public override string ToString() => $"{PLUGIN_NAME}: {Button}";
    }
}
