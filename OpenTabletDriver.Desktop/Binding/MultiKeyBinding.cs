using System;
using System.Collections.Generic;
using System.Linq;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Platform.Keyboard;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Binding
{
    [PluginName(PLUGIN_NAME)]
    public class MultiKeyBinding : IStateBinding
    {
        private readonly InputDevice _device;
        private readonly IVirtualKeyboard _keyboard;

        public MultiKeyBinding(InputDevice device, IVirtualKeyboard keyboard)
        {
            _device = device;
            _keyboard = keyboard;
        }

        private const string PLUGIN_NAME = "Multi-Key Binding";
        private const char KEYS_SPLITTER = '+';

        private IList<string> _keys;
        private string _keysString;

        [Property("Keys")]
        public string Keys
        {
            set
            {
                _keysString = value;
                _keys = ParseKeys(Keys);
            }
            get => _keysString;
        }

        public void Press(IDeviceReport report)
        {
            if (_keys.Count > 0)
                _keyboard.Press(_keys);
        }

        public void Release(IDeviceReport report)
        {
            if (_keys.Count > 0)
                _keyboard.Release(_keys);
        }

        private IList<string> ParseKeys(string str)
        {
            var newKeys = str.Split(KEYS_SPLITTER, StringSplitOptions.TrimEntries);
            return newKeys.All(k => _keyboard.SupportedKeys.Contains(k)) ? newKeys : Array.Empty<string>();
        }

        public override string ToString() => $"{PLUGIN_NAME}: {Keys}";
    }
}
