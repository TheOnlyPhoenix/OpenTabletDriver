using System.Numerics;
using JetBrains.Annotations;

namespace OpenTabletDriver
{
    /// <summary>
    /// A working area designating width and height based at a centered origin position.
    /// </summary>
    [PublicAPI]
    public class Area
    {
        /// <summary>
        /// The width of the area.
        /// </summary>
        public float Width { set; get; }

        /// <summary>
        /// The height of the area.
        /// </summary>
        public float Height { set; get; }

        /// <summary>
        /// The center offset of the area.
        /// </summary>
        /// <remarks>
        /// This is also the rotation origin of the area where applicable.
        /// </remarks>
        public Vector2 Position { set; get; }

        public override string ToString() => $"[{Width}x{Height}@{Position}]";
    }
}
