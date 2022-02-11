using System.Numerics;

namespace OpenTabletDriver
{
    public class AngledArea : Area
    {
        /// <summary>
        /// The rotation angle of the area.
        /// </summary>
        public float Rotation { set; get; } = 0;

        public override string ToString() => $"[{Width}x{Height}@{Position}:{Rotation}°]";
    }
}
