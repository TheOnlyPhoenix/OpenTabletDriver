using System.Collections.Generic;

namespace OpenTabletDriver.Platform.Keyboard
{
    public interface IVirtualKeyboard
    {
        void Press(string key);
        void Release(string key);

        void Press(IEnumerable<string> keys);
        void Release(IEnumerable<string> keys);

        IEnumerable<string> SupportedKeys { get; }
    }
}
