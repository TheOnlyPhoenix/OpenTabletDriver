using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Attributes;

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

            serviceCollection.RemoveServices(existingConflictingServices);

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
