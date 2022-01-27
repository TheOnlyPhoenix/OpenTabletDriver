namespace OpenTabletDriver.Tablet.Touch
{
    public interface ITouchReport : IDeviceReport
    {
        TouchPoint[] Touches { get; }
    }
}
