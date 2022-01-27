using System.Numerics;

namespace OpenTabletDriver.Tablet.Touch
{
    public class TouchPoint
    {
        public byte TouchID;
        public Vector2 Position;

        public override string ToString()
        {
            return $"Point #{TouchID}: {Position};";
        }
    }
}
