using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using OpenTabletDriver.Attributes;

#nullable enable

namespace OpenTabletDriver.Desktop.Reflection
{
    public class PluginSettingStore
    {
        public PluginSettingStore(Type type, bool enable = true)
        {
            Path = type.FullName!;
            Settings = GetSettingsForType(type);
            Enable = enable;
        }

        public PluginSettingStore(Type type, object settings, bool enable = true)
        {
            Path = type.FullName!;
            Settings = GetSettingsFromObject(settings);
            Enable = enable;
        }

        public PluginSettingStore(object? source, bool enable = true)
        {
            if (source != null)
            {
                var sourceType = source.GetType();
                Path = sourceType.FullName;
                Settings = GetSettingsForType(sourceType, source);
                Enable = enable;
            }
            else
            {
                throw new NullReferenceException("Creating a plugin setting store from a null object is not allowed.");
            }
        }

        [JsonConstructor]
        private PluginSettingStore()
        {
        }

        public string? Path { set; get; }

        public ObservableCollection<PluginSetting>? Settings { get; }

        public bool Enable { set; get; }

        public PluginSetting this[string propertyName]
        {
            set
            {
                if (Settings?.FirstOrDefault(t => t.Property == propertyName) is PluginSetting setting)
                {
                    Settings.Remove(setting);
                    Settings.Add(value);
                }
                else
                {
                    Settings!.Add(value);
                }
            }
            get
            {
                var result = Settings!.FirstOrDefault(s => s.Property == propertyName);
                if (result == null)
                {
                    var newSetting = new PluginSetting(propertyName, null);
                    Settings!.Add(newSetting);
                    return newSetting;
                }
                return result;
            }
        }

        private static ObservableCollection<PluginSetting> GetSettingsForType(Type targetType, object? source = null)
        {
            var settings = from property in targetType.GetProperties()
                where property.GetCustomAttribute<PropertyAttribute>() != null
                select new PluginSetting(property, source == null ? null : property.GetValue(source));

            return new ObservableCollection<PluginSetting>(settings);
        }

        private static ObservableCollection<PluginSetting> GetSettingsFromObject(object obj)
        {
            var type = obj.GetType();
            var properties = type.GetProperties();

            var settings = from property in properties
                let name = property.Name
                let value = property.GetValue(obj)
                select new PluginSetting(name, value);

            return new ObservableCollection<PluginSetting>(settings);
        }
    }
}
