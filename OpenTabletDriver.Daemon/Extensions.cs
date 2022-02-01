using System.Linq;
using System.Reflection;
using OpenTabletDriver.Attributes;
using OpenTabletDriver.Desktop.Reflection;

#nullable enable

namespace OpenTabletDriver.Daemon
{
    public static class Extensions
    {
        public static T? Construct<T>(this IPluginFactory pluginFactory, PluginSettingStore? settingStore, params object[] args) where T : class
        {
            if (settingStore == null)
                return null;

            var obj = pluginFactory.Construct<T>(settingStore.Path!, args);
            settingStore.ApplySettings(obj);
            return obj;
        }

        public static string? GetName(this IPluginFactory pluginFactory, PluginSettingStore? settingStore) => GetName(pluginFactory, settingStore?.Path);

        public static string? GetName(this IPluginFactory pluginFactory, string? path)
        {
            if (path == null)
                return null;

            var type = pluginFactory.GetPluginType(path);
            var attr = type?.GetCustomAttribute<PluginNameAttribute>();
            return attr != null ? attr.Name : type?.Name;
        }

        public static void ApplySettings(this PluginSettingStore store, object? obj)
        {
            if (obj == null)
                return;

            var type = obj.GetType();
            foreach (var property in type.GetProperties())
            {
                var attr = property.GetCustomAttribute<PropertyAttribute>();
                if (attr != null)
                {
                    var setting = store[property.Name];
                    property.SetValue(obj, setting.GetValue(property.PropertyType));
                }
            }
        }
    }
}
