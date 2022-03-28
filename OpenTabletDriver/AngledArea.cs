using System;
using System.Linq;
using System.Numerics;
using JetBrains.Annotations;

namespace OpenTabletDriver
{
    /// <summary>
    /// An <see cref="Area"/> supporting rotation.
    /// </summary>
    [PublicAPI]
    public class AngledArea : Area
    {
        /// <summary>
        /// The rotation angle of the area.
        /// </summary>
        public float Rotation { set; get; }

        public override Vector2[] Corners
        {
            get
            {
                var origin = Position;
                var matrix = Matrix3x2.CreateTranslation(-origin);
                matrix *= Matrix3x2.CreateRotation((float)(Rotation * Math.PI / 180));
                matrix *= Matrix3x2.CreateTranslation(origin);

                var transformedCorners = from corner in base.Corners
                    select Vector2.Transform(corner, matrix);

                return transformedCorners.ToArray();
            }
        }

        public override string ToString() => $"[{Width}x{Height}@{Position}:{Rotation}°]";
    }
}
