using System.Collections.Generic;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Diagnostics;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.Json.Converters;
using OpenTabletDriver.Desktop.Json.Converters.Implementations;
using OpenTabletDriver.Devices;

namespace OpenTabletDriver.Desktop.Json
{
    internal static class Utilities
    {
        internal static readonly IEnumerable<JsonConverter> Converters = new JsonConverter[]
        {
            new InterfaceConverter<IDeviceEndpoint, SerializableDeviceEndpoint>(),
            new InterfaceConverter<IAppInfo, SerializableAppInfo>(),
            new InterfaceConverter<IDiagnosticInfo, SerializableDiagnosticInfo>(),
        };
    }
}
