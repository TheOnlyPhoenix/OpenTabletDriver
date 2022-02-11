using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop;

namespace OpenTabletDriver.Daemon
{
    public class DaemonBuilder : HostBuilder<DriverDaemon>
    {
        public DaemonBuilder() : base(DesktopServiceCollection.GetPlatformServiceCollection())
        {
        }

        public override DriverDaemon Build(out IServiceProvider serviceProvider)
        {
            serviceProvider = BuildServiceProvider();
            return ActivatorUtilities.CreateInstance<DriverDaemon>(serviceProvider);
        }
    }
}

