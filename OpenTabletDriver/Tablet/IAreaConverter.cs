namespace OpenTabletDriver.Tablet
{
    public interface IAreaConverter
    {
        DeviceVendor Vendor { get; }

        string Top { get; }
        string Left { get; }
        string Bottom { get; }
        string Right { get; }

        AngledArea Convert(InputDevice tablet, double top, double left, double bottom, double right);
    }
}
