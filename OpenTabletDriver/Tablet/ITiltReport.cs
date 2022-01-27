using System.Numerics;

namespace OpenTabletDriver.Tablet
{
    public interface ITiltReport : IDeviceReport
    {
        Vector2 Tilt { set; get; }
    }
}
