using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using OpenTabletDriver.Desktop.Interop.AppInfo;
using OpenTabletDriver.Desktop.Migration;
using OpenTabletDriver.Desktop.Profiles;
using OpenTabletDriver.Desktop.Reflection;

namespace OpenTabletDriver.Desktop
{
    public class Settings : ViewModel
    {
        private ProfileCollection _profiles;
        private bool _lockUsableAreaDisplay, _lockUsableAreaTablet;
        private PluginSettingStoreCollection _tools = new PluginSettingStoreCollection();

        [JsonProperty("Profiles")]
        public ProfileCollection Profiles
        {
            set => this.RaiseAndSetIfChanged(ref _profiles, value);
            get => _profiles;
        }

        [JsonProperty("LockUsableAreaDisplay")]
        public bool LockUsableAreaDisplay
        {
            set => this.RaiseAndSetIfChanged(ref this._lockUsableAreaDisplay, value);
            get => this._lockUsableAreaDisplay;
        }

        [JsonProperty("LockUsableAreaTablet")]
        public bool LockUsableAreaTablet
        {
            set => this.RaiseAndSetIfChanged(ref this._lockUsableAreaTablet, value);
            get => this._lockUsableAreaTablet;
        }

        [JsonProperty("Tools")]
        public PluginSettingStoreCollection Tools
        {
            set => RaiseAndSetIfChanged(ref this._tools, value);
            get => this._tools;
        }

        public static Settings GetDefaults(IServiceProvider serviceProvider)
        {
            return new Settings
            {
                Profiles = new ProfileCollection(serviceProvider),
                LockUsableAreaDisplay = true,
                LockUsableAreaTablet = true
            };
        }

        #region Custom Serialization

        static Settings()
        {
            Serializer.Error += SerializationErrorHandler;
        }

        private static readonly JsonSerializer Serializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };

        private static void SerializationErrorHandler(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
        {
            args.ErrorContext.Handled = true;
            if (args.ErrorContext.Path is string path)
            {
                if (args.CurrentObject == null)
                    return;

                var property = args.CurrentObject.GetType().GetProperty(path);
                if (property != null && property.PropertyType == typeof(PluginSettingStore))
                {
                    var match = propertyValueRegex.Match(args.ErrorContext.Error.Message);
                    if (match.Success)
                    {
                        // TODO: Fix settings auto migration
                        // var objPath = SettingsMigrator.MigrateNamespace(match.Groups[1].Value);
                        // var newValue = PluginSettingStore.FromPath(objPath);
                        // if (newValue != null)
                        // {
                        //     property.SetValue(args.CurrentObject, newValue);
                        //     Log.Write("Settings", $"Migrated {path} to {nameof(PluginSettingStore)}");
                        //     return;
                        // }
                        Log.Write("Settings", "Ignoring failed migration temporarily.", LogLevel.Error);
                        return;
                    }
                }
                Log.Write("Settings", $"Unable to migrate {path}", LogLevel.Error);
                return;
            }
            Log.Exception(args.ErrorContext.Error);
        }

        private static Regex propertyValueRegex = new Regex(PROPERTY_VALUE_REGEX, RegexOptions.Compiled);
        private const string PROPERTY_VALUE_REGEX = "\\\"(.+?)\\\"";

        public static Settings Deserialize(FileInfo file)
        {
            using (var stream = file.OpenRead())
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
                return Serializer.Deserialize<Settings>(jr);
        }

        public static void Recover(FileInfo file, Settings settings)
        {
            using (var stream = file.OpenRead())
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
            {
                void PropertyWatch(object _, PropertyChangedEventArgs p)
                {
                    var prop = settings.GetType().GetProperty(p.PropertyName).GetValue(settings);
                    Log.Write("Settings", $"Recovered '{p.PropertyName}'", LogLevel.Debug);
                }

                settings.PropertyChanged += PropertyWatch;

                try
                {
                    Serializer.Populate(jr, settings);
                }
                catch (JsonReaderException e)
                {
                    Log.Write("Settings", $"Recovery ended. Reason: {e.Message}", LogLevel.Debug);
                }

                settings.PropertyChanged -= PropertyWatch;
            }
        }

        public void Serialize(FileInfo file)
        {
            try
            {
                if (file.Exists)
                    file.Delete();

                using (var sw = file.CreateText())
                using (var jw = new JsonTextWriter(sw))
                    Serializer.Serialize(jw, this);
            }
            catch (UnauthorizedAccessException)
            {
                Log.Write("Settings", $"OpenTabletDriver doesn't have permission to save persistent settings to {file.DirectoryName}", LogLevel.Error);
            }
        }

        #endregion
    }
}
