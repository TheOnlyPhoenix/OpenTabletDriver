using OpenTabletDriver.Attributes;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Relative Mode")]
    public class RelativeMode : RelativeOutputMode, IPointerProvider<IRelativePointer>
    {
        public RelativeMode(
            InputDevice tablet,
            IRelativePointer relativePointer,
            ISettingsProvider settingsProvider
        ) : base(tablet, relativePointer)
        {
            settingsProvider.Inject(this);
        }
    }
}
