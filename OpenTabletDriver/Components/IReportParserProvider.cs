using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Components
{
    public interface IReportParserProvider
    {
        IReportParser<IDeviceReport> GetReportParser(string reportParserName);
    }
}
