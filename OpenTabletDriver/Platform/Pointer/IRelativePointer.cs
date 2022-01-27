using System.Numerics;

namespace OpenTabletDriver.Platform.Pointer
{
    public interface IRelativePointer
    {
        void SetPosition(Vector2 delta);
    }
}
