using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTabletDriver.Tablet;

#nullable enable

namespace OpenTabletDriver.Desktop.RPC
{
    [JsonObject]
    public class DebugReportData
    {
        [JsonConstructor]
        public DebugReportData()
        {
            Data = null!;
        }

        public DebugReportData(IDeviceReport report)
        {
            Data = JToken.FromObject(report);
            Path = report.GetType().FullName!;
        }

        public string Path { set; get; } = string.Empty;
        public JToken Data { set; get; }
    }
}
