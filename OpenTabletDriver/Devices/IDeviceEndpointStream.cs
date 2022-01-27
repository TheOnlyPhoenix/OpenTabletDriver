using System;

namespace OpenTabletDriver.Devices
{
    public interface IDeviceEndpointStream : IDisposable
    {
        byte[] Read();
        void Write(byte[] buffer);

        void GetFeature(byte[] buffer);
        void SetFeature(byte[] buffer);
    }
}
