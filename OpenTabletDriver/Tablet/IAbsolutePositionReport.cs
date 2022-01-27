using System.Numerics;

namespace OpenTabletDriver.Tablet
{
    public interface IAbsolutePositionReport : IDeviceReport
    {
        Vector2 Position { get; set; }
    }
}
