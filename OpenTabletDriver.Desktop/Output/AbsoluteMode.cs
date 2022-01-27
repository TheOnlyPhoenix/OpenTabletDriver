using OpenTabletDriver.Attributes;
using OpenTabletDriver.DependencyInjection;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Absolute Mode")]
    public class AbsoluteMode : AbsoluteOutputMode, IPointerProvider<IAbsolutePointer>
    {
        [Resolved]
        public override IAbsolutePointer Pointer { set; get; }
    }
}
