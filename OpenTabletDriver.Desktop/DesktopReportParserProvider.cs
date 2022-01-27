using OpenTabletDriver.Components;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop
{
    public class DesktopReportParserProvider : IReportParserProvider
    {
        public IReportParser<IDeviceReport> GetReportParser(string reportParserName)
        {
            return AppInfo.PluginManager.ConstructObject<IReportParser<IDeviceReport>>(reportParserName);
        }
    }
}
