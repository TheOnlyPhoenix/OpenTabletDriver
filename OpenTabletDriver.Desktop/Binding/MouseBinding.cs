using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Platform.Pointer;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class MouseBinding : IStateBinding
    {
        private readonly InputDevice _device;
        private readonly IMouseButtonHandler _pointer;

        public MouseBinding(InputDevice device, IMouseButtonHandler pointer)
        {
            _device = device;
            _pointer = pointer;
        }

        private const string PLUGIN_NAME = "Mouse Button Binding";

        [Property("Button"), MemberValidated(nameof(ValidButtons))]
        public string Button { set; get; }

        public void Press(IDeviceReport report)
        {
            if (Enum.TryParse<MouseButton>(Button, true, out var mouseButton))
                _pointer?.MouseDown(mouseButton);
        }

        public void Release(IDeviceReport report)
        {
            if (Enum.TryParse<MouseButton>(Button, true, out var mouseButton))
                _pointer?.MouseUp(mouseButton);
        }

        private static IEnumerable<string> validButtons;
        public static IEnumerable<string> ValidButtons =>
            validButtons ??= Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>().Select(Enum.GetName);

        public override string ToString() => $"{PLUGIN_NAME}: {Button}";
    }
}
