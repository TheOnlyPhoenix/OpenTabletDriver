using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.RPC
{
    public class DebugReportData
    {
        [JsonConstructor]
        public DebugReportData()
        {
        }

        public DebugReportData(IDeviceReport report)
        {
            Data = JToken.FromObject(report);
            Path = report.GetType().FullName;
        }

        public string Path { set; get; }
        public JToken Data { set; get; }
    }
}
