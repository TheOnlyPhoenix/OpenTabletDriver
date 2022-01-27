namespace OpenTabletDriver.Tablet
{
    public interface IProximityReport : IDeviceReport
    {
        bool NearProximity { set; get; }
        uint HoverDistance { set; get; }
    }
}
