using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace OpenTabletDriver.Desktop
{
    public static class DependencyInjectionExtensions
    {
        private const string LOG_GROUP = nameof(DesktopServiceCollection);

        public static void AddServices(this IServiceCollection serviceCollection, IEnumerable<ServiceDescriptor> services)
        {
            var existingServiceTypes = serviceCollection.Select(s => s.ServiceType);
            var newServiceTypes = services.Select(s => s.ServiceType);
            var conflictingServiceTypes = existingServiceTypes.Intersect(newServiceTypes);

            var existingConflictingServices = serviceCollection.Where(
                s => conflictingServiceTypes.Contains(s.ServiceType)
            );

            serviceCollection.RemoveServices(existingConflictingServices.ToImmutableArray());

            foreach (var service in services)
            {
                serviceCollection.Add(service);
            }
        }

        public static void RemoveServices(this IServiceCollection serviceCollection, IEnumerable<ServiceDescriptor> services)
        {
            foreach (var service in services)
            {
                serviceCollection.Remove(service);
            }
        }
    }
}
