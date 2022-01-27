using System.Collections.Generic;

namespace OpenTabletDriver.Platform.Display
{
    public interface IVirtualScreen : IDisplay
    {
        IEnumerable<IDisplay> Displays { get; }
    }
}
