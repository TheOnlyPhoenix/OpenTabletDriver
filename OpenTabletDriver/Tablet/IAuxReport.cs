namespace OpenTabletDriver.Tablet
{
    public interface IAuxReport : IDeviceReport
    {
        bool[] AuxButtons { set; get; }
    }
}
