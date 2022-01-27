using System;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.DependencyInjection;
using OpenTabletDriver.Output;
using OpenTabletDriver.Platform.Pointer;

namespace OpenTabletDriver.Desktop.Output
{
    [PluginName("Artist Mode"), SupportedPlatform(SystemPlatform.Linux)]
    public class LinuxArtistMode : AbsoluteOutputMode, IPointerProvider<IAbsolutePointer>
    {
        [Resolved]
        public IPressureHandler VirtualTablet { get; set; }

        public override IAbsolutePointer Pointer
        {
            set => throw new NotSupportedException();
            get => (IAbsolutePointer)VirtualTablet;
        }
    }
}
