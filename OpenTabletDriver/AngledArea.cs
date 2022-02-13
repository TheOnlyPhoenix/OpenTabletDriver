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

        public override string ToString() => $"[{Width}x{Height}@{Position}:{Rotation}°]";
    }
}
