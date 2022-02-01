using OpenTabletDriver.Attributes;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Absolute Mode")]
    public class AbsoluteMode : AbsoluteOutputMode, IPointerProvider<IAbsolutePointer>
    {
        public AbsoluteMode(InputDevice tablet, IAbsolutePointer absolutePointer) : base(tablet, absolutePointer)
        {
        }
    }
}
