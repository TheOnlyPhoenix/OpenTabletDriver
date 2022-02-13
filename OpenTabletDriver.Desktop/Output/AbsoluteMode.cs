using OpenTabletDriver.Attributes;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Absolute Mode")]
    public class AbsoluteMode : AbsoluteOutputMode
    {
        public AbsoluteMode(
            InputDevice tablet,
            IAbsolutePointer absolutePointer,
            ISettingsProvider settingsProvider
        ) : base(tablet, absolutePointer)
        {
            settingsProvider.Inject(this);
        }
    }
}
