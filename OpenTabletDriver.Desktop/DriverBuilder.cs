using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Desktop.Interop;
using OpenTabletDriver.Exceptions;
using OpenTabletDriver.Interop;

#nullable enable

namespace OpenTabletDriver.Desktop
{
    public class DriverBuilder<T> : IHostBuilder<T> where T : IDriver
    {
        private readonly DesktopServiceCollection _serviceCollection;
        private bool _built;

        public DriverBuilder()
        {
            _serviceCollection = DesktopServiceCollection.GetPlatformServiceCollection();
        }

        public DriverBuilder(DesktopServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public IHostBuilder<T> ConfigureServices(Action<IServiceCollection> configure)
        {
            if (_built)
                throw new DriverAlreadyBuiltException();

            configure(_serviceCollection);
            return this;
        }

        public T Build(out IServiceProvider serviceProvider)
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

            return ActivatorUtilities.CreateInstance<T>(serviceProvider);
        }
    }
}
