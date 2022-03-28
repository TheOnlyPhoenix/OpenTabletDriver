using System.Linq;
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

        /// <summary>
        /// All corners of the area.
        /// </summary>
        public virtual Vector2[] Corners
        {
            get
            {
                var halfWidth = Width / 2;
                var halfHeight = Height / 2;

                var x = Position.X;
                var y = Position.Y;

                return new[]
                {
                    new Vector2(x - halfWidth, y - halfHeight),
                    new Vector2(x - halfWidth, y + halfHeight),
                    new Vector2(x + halfWidth, y + halfHeight),
                    new Vector2(x + halfWidth, y - halfHeight)
                };
            }
        }

        /// <summary>
        /// The center offset of the area.
        /// </summary>
        public virtual Vector2 CenterOffset
        {
            get
            {
                var max = new Vector2(
                    Corners.Max(v => v.X),
                    Corners.Max(v => v.Y)
                );
                var min = new Vector2(
                    Corners.Min(v => v.X),
                    Corners.Min(v => v.Y)
                );
                return (max - min) / 2;
            }
        }

        public override string ToString() => $"[{Width}x{Height}@{Position}]";
    }
}
