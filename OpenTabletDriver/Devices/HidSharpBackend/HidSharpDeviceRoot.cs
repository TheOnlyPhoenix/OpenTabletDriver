using System;
using System.Collections.Generic;
using System.Linq;
using HidSharp;

namespace OpenTabletDriver.Devices.HidSharpBackend
{
    [Attributes.DeviceHub]
    public sealed class HidSharpDeviceRootHub : IDeviceHub
    {
        public HidSharpDeviceRootHub()
        {
            DeviceList.Local.Changed += (sender, e) =>
            {
                var newList = DeviceList.Local.GetHidDevices().Select(d => new HidSharpEndpoint(d));
                var changes = new DevicesChangedEventArgs(_hidDevices, newList);
                if (changes.Changes.Any())
                {
                    DevicesChanged?.Invoke(this, changes);
                    _hidDevices = newList;
                }
            };
        }

        private IEnumerable<IDeviceEndpoint> _hidDevices = DeviceList.Local.GetHidDevices().Select(d => new HidSharpEndpoint(d));

        public event EventHandler<DevicesChangedEventArgs> DevicesChanged;

        public IEnumerable<IDeviceEndpoint> GetDevices()
        {
            return DeviceList.Local.GetHidDevices().Select(d => new HidSharpEndpoint(d));
        }
    }
}
