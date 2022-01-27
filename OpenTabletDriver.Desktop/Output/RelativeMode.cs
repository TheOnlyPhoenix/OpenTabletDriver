using OpenTabletDriver.Attributes;
using OpenTabletDriver.DependencyInjection;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Relative Mode")]
    public class RelativeMode : RelativeOutputMode, IPointerProvider<IRelativePointer>
    {
        [Resolved]
        public override IRelativePointer Pointer { set; get; }
    }
}
