using System.Numerics;

namespace OpenTabletDriver.Platform.Pointer
{
    public interface IAbsolutePointer
    {
        void SetPosition(Vector2 pos);
    }
}
