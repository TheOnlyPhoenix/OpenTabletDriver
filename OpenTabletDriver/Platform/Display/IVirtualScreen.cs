using System.Collections.Generic;

#nullable enable

namespace OpenTabletDriver.Platform.Display
{
    public interface IVirtualScreen : IDisplay
    {
        IEnumerable<IDisplay> Displays { get; }
    }
}
