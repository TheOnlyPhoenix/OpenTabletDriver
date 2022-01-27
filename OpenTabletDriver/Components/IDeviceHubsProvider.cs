using System.Collections.Generic;
using OpenTabletDriver.Devices;

namespace OpenTabletDriver.Components
{
    public interface IDeviceHubsProvider
    {
        IEnumerable<IDeviceHub> DeviceHubs { get; }
    }
}
