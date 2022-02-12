using System;
using System.Collections.Generic;

#nullable enable

namespace OpenTabletDriver.Devices
{
    public interface IDeviceHub
    {
        event EventHandler<DevicesChangedEventArgs>? DevicesChanged;

        IEnumerable<IDeviceEndpoint> GetDevices();
    }
}
