using System.Collections.Generic;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Components
{
    public interface IDeviceConfigurationProvider
    {
        IEnumerable<TabletConfiguration> TabletConfigurations { get; }
    }
}
