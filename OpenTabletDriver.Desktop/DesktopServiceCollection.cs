using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTabletDriver.ComponentProviders;
using OpenTabletDriver.Components;
using OpenTabletDriver.Configurations;
using OpenTabletDriver.Devices;

#nullable enable

namespace OpenTabletDriver.Desktop
{
    public class DesktopServiceCollection : ServiceCollection
    {
        private static IEnumerable<ServiceDescriptor> RequiredServices => new ServiceDescriptor[]
        {
            ServiceDescriptor.Singleton<IReportParserProvider, ReportParserProvider>(),
            ServiceDescriptor.Singleton<IDeviceHubsProvider, DeviceHubsProvider>(serviceProvider => new DeviceHubsProvider(serviceProvider)),
            ServiceDescriptor.Singleton<ICompositeDeviceHub, RootHub>(serviceProvider => RootHub.WithProvider(serviceProvider)),
            ServiceDescriptor.Singleton<IDeviceConfigurationProvider, DeviceConfigurationProvider>()
        };

        public DesktopServiceCollection()
        {
            foreach (var serviceDescriptor in RequiredServices)
                this.Add(serviceDescriptor);
        }
    }
}
