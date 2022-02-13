using JetBrains.Annotations;

namespace OpenTabletDriver.Tablet
{
    /// <summary>
    /// Device specifications for a tablet device.
    /// </summary>
    [PublicAPI]
    public class TabletSpecifications
    {
        /// <summary>
        /// Specifications for the tablet digitizer.
        /// </summary>
        public DigitizerSpecifications Digitizer { set; get; } = new DigitizerSpecifications();

        /// <summary>
        /// Specifications for the tablet's pen.
        /// </summary>
        public PenSpecifications Pen { set; get; } = new PenSpecifications();

        /// <summary>
        /// Specifications for the auxiliary buttons.
        /// </summary>
        public ButtonSpecifications AuxiliaryButtons { set; get; } = new ButtonSpecifications();

        /// <summary>
        /// Specifications for the mouse buttons.
        /// </summary>
        public ButtonSpecifications MouseButtons { set; get; } = new ButtonSpecifications();

        /// <summary>
        /// Specifications for the touch digitizer.
        /// </summary>
        public DigitizerSpecifications Touch { set; get; } = new DigitizerSpecifications();
    }
}
