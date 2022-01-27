using System.Numerics;

namespace OpenTabletDriver.Tablet
{
    public interface IMouseReport : IAbsolutePositionReport
    {
        bool[] MouseButtons { set; get; }
        Vector2 Scroll { set; get; }
    }
}
