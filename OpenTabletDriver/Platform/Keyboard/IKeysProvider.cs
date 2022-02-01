using System.Collections.Generic;

namespace OpenTabletDriver.Platform.Keyboard
{
    public interface IKeysProvider
    {
        IDictionary<string, object> EtoToNative { get; }
    }
}
