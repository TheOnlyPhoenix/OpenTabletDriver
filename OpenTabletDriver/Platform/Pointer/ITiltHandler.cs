using System.Numerics;

namespace OpenTabletDriver.Platform.Pointer
{
    public interface ITiltHandler
    {
        void SetTilt(Vector2 tilt);
    }
}
