using System.Collections.Generic;
using OpenTabletDriver.Devices;

namespace OpenTabletDriver.Components
{
    public interface ICompositeDeviceHub : IDeviceHub
    {
        IEnumerable<IDeviceHub> DeviceHubs { get; }
        void ConnectDeviceHub<T>() where T : IDeviceHub;
        void ConnectDeviceHub(IDeviceHub instance);
        void DisconnectDeviceHub<T>() where T : IDeviceHub;
        void DisconnectDeviceHub(IDeviceHub instance);
    }
}
