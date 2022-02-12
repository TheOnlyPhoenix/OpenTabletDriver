using HidSharp;

#nullable enable

namespace OpenTabletDriver.Devices.HidSharpBackend
{
    public sealed class HidSharpEndpointStream : IDeviceEndpointStream
    {
        internal HidSharpEndpointStream(HidStream stream)
        {
            _stream = stream;
            _stream.ReadTimeout = int.MaxValue;
        }

        private readonly HidStream _stream;

        public byte[] Read() => _stream.Read();
        public void Write(byte[] buffer) => _stream.Write(buffer);

        public void GetFeature(byte[] buffer) => _stream.GetFeature(buffer);
        public void SetFeature(byte[] buffer) => _stream.SetFeature(buffer);

        public void Dispose() => _stream.Dispose();
    }
}
