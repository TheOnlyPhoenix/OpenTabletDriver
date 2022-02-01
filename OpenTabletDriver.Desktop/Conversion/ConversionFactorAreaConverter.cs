using System.Numerics;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Tablet;

namespace OpenTabletDriver.Desktop.Conversion
{
    [PluginName("Wacom, VEIKK")]
    public class ConversionFactorAreaConverter : IAreaConverter
    {
        public virtual DeviceVendor Vendor => DeviceVendor.Wacom & DeviceVendor.VEIKK;

        public string Top => "Top";
        public string Left => "Left";
        public string Bottom => "Bottom";
        public string Right => "Right";

        protected double GetConversionFactor(InputDevice tablet)
        {
            var digitizer = tablet.Properties.Specifications.Digitizer;
            return digitizer.MaxX / digitizer.Width;
        }

        public Area Convert(InputDevice tablet, double top, double left, double bottom, double right)
        {
            double conversionFactor = GetConversionFactor(tablet);
            var width = (right - left) / conversionFactor;
            var height = (bottom - top) / conversionFactor;
            var offsetX = (width / 2) + (left / conversionFactor);
            var offsetY = (height / 2) + (top / conversionFactor);

            return new Area((float)width, (float)height, new Vector2((float)offsetX, (float)offsetY), 0f);
        }
    }
}
