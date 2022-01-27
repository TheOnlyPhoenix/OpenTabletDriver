namespace OpenTabletDriver.Tablet
{
    public interface IEraserReport : IDeviceReport
    {
        bool Eraser { set; get; }
    }
}
