using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// Device specifications for buttons.
    /// </summary>
    [PublicAPI]
    public class ButtonSpecifications
    {
        /// <summary>
        /// The amount of buttons.
        /// </summary>
        public uint ButtonCount { set; get; }
    }
}
