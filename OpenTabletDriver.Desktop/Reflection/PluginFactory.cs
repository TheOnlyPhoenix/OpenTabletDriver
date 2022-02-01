using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Logging;

#nullable enable

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginFactory : IPluginFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IReadOnlyList<TypeInfo> _pluginTypes;
        private readonly IReadOnlyList<TypeInfo> _libraryTypes;

        public PluginFactory(IServiceProvider serviceProvider, IPluginManager pluginManager)
        {
            _serviceProvider = serviceProvider;

            var internalTypes = from asm in pluginManager.Assemblies
                                from type in asm.DefinedTypes
                                where type.IsPublic && !(type.IsInterface || type.IsAbstract)
                                where IsPluginType(type)
                                where IsPlatformSupported(type)
                                select type;

            var libTypesQuery = from assembly in pluginManager.Assemblies
                from type in assembly.GetExportedTypes()
                where type.IsAbstract || type.IsInterface
                select type.GetTypeInfo();

            _libraryTypes = libTypesQuery.ToImmutableArray();
            _pluginTypes = internalTypes.Concat(_libraryTypes).ToImmutableArray();
        }

        public T? Construct<T>(string name, params object[] args) where T : class
        {
            // TODO: handle plugin settings
            var type = GetPluginType(name);
            if (type != null)
            {
                try
                {
                    return ActivatorUtilities.CreateInstance(_serviceProvider, type, args) as T;
                }
                catch (TargetInvocationException e) when (e.Message == "Exception has been thrown by the target of an invocation.")
                {
                    Log.Write("Plugin", "Object construction has thrown an error", LogLevel.Error);
                    Log.Exception(e.InnerException);
                }
                catch (Exception e)
                {
                    Log.Write("Plugin", $"Unable to construct object '{name}'", LogLevel.Error);
                    Log.Exception(e);
                }
            }
            else
            {
                Log.Write("Plugin", $"No constructor found for '{name}'", LogLevel.Error);
            }

            return null;
        }

        public TypeInfo? GetPluginType(string name)
        {
            return _pluginTypes.FirstOrDefault(t => t.FullName == name);
        }

        private bool IsPluginType(Type type)
        {
            return !type.IsAbstract && !type.IsInterface &&
                _libraryTypes.Any(t => t.IsAssignableFrom(type) ||
                    type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == t));
        }

        private static bool IsPlatformSupported(Type type)
        {
            var attr = type.GetCustomAttribute(typeof(SupportedPlatformAttribute), false) as SupportedPlatformAttribute;
            return attr?.IsCurrentPlatform ?? true;
        }

        private static bool IsPluginIgnored(Type type)
        {
            return type.GetCustomAttributes(false).Any(a => a.GetType() == typeof(PluginIgnoreAttribute));
        }

        private static bool IsLoadable(Assembly asm)
        {
            try
            {
                _ = asm.DefinedTypes;
                return true;
            }
            catch (Exception ex)
            {
                var asmName = asm.GetName();
                var hResultHex = ex.HResult.ToString("X");
                var message = new LogMessage
                {
                    Group = "Plugin",
                    Level = LogLevel.Warning,
                    Message = $"Plugin '{asmName.Name}, Version={asmName.Version}' can't be loaded and is likely out of date. (HResult: 0x{hResultHex})",
                    StackTrace = ex.Message + Environment.NewLine + ex.StackTrace
                };
                Log.Write(message);
                return false;
            }
        }
    }
}
