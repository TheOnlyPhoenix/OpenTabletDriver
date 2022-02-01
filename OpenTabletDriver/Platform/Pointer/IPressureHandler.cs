namespace OpenTabletDriver.Platform.Pointer
{
    public interface IPressureHandler : IAbsolutePointer
    {
        void SetPressure(float percentage);
    }
}
