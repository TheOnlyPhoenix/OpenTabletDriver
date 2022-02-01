using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop;

namespace OpenTabletDriver.Daemon
{
    public class DaemonBuilder : IHostBuilder<DriverDaemon>
    {
        private readonly IServiceCollection _serviceCollection;

        public DaemonBuilder()
        {
            _serviceCollection = DesktopServiceCollection.GetPlatformServiceCollection();
        }

        private bool _built;

        public IHostBuilder<DriverDaemon> ConfigureServices(Action<IServiceCollection> configure)
        {
            if (_built)
                throw new InvalidOperationException("Cannot configure services after the daemon has been built.");

            configure(_serviceCollection);
            return this;
        }

        public DriverDaemon Build(out IServiceProvider serviceProvider)
        {
            if (_built)
                throw new InvalidOperationException("Cannot build service from the same service collection more than once.");

#if DEBUG
            serviceProvider = _serviceCollection.BuildServiceProvider(new ServiceProviderOptions()
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });
#else
            serviceProvider = _serviceCollection.BuildServiceProvider();
#endif
            _built = true;

            return ActivatorUtilities.CreateInstance<DriverDaemon>(serviceProvider);
        }
    }
}

