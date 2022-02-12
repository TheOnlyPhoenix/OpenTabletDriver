using OpenTabletDriver.Desktop.Interop.AppInfo;

#nullable enable

namespace OpenTabletDriver.Desktop.Json.Converters.Implementations
{
    internal sealed class SerializableAppInfo : Serializable, IAppInfo
    {
        public string ConfigurationDirectory { set; get; } = string.Empty;
        public string SettingsFile { set; get; } = string.Empty;
        public string PluginDirectory { set; get; } = string.Empty;
        public string PresetDirectory { set; get; } = string.Empty;
        public string TemporaryDirectory { set; get; } = string.Empty;
        public string CacheDirectory { set; get; } = string.Empty;
        public string BackupDirectory { set; get; } = string.Empty;
        public string TrashDirectory { set; get; } = string.Empty;
        public string AppDataDirectory { set; get; } = string.Empty;
    }
}
