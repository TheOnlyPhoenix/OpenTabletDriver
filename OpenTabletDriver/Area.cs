using System.Numerics;

#nullable enable

namespace OpenTabletDriver
{
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
        /// This is also the rotation point of the area.
        /// </remarks>
        public Vector2 Position { set; get; }

        public override string ToString() => $"[{Width}x{Height}@{Position}]";
    }
}
