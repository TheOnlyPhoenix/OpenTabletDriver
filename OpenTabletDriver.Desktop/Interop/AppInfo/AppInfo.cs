using System;
using System.IO;
using System.Linq;

namespace OpenTabletDriver.Desktop.Interop.AppInfo
{
    public class AppInfo : IAppInfo
    {
        private string _configurationDirectory,
            _settingsFile,
            _pluginDirectory,
            _presetDirectory,
            _temporaryDirectory,
            _cacheDirectory,
            _backupDirectory,
            _trashDirectory;

        public string AppDataDirectory { set; get; }

        public string ConfigurationDirectory
        {
            set => _configurationDirectory = value;
            get => _configurationDirectory ?? GetDefaultConfigurationDirectory();
        }

        public string SettingsFile
        {
            set => _settingsFile = value;
            get => _settingsFile ?? GetDefaultSettingsFile();
        }

        public string PluginDirectory
        {
            set => _pluginDirectory = value;
            get => _pluginDirectory ?? GetDefaultPluginDirectory();
        }

        public string PresetDirectory
        {
            set => _presetDirectory = value;
            get => _presetDirectory ?? GetDefaultPresetDirectory();
        }

        public string TemporaryDirectory
        {
            set => _temporaryDirectory = value;
            get => _temporaryDirectory ?? GetDefaultTemporaryDirectory();
        }

        public string CacheDirectory
        {
            set => _cacheDirectory = value;
            get => _cacheDirectory ?? GetDefaultCacheDirectory();
        }

        public string BackupDirectory
        {
            set => _backupDirectory = value;
            get => _backupDirectory ?? GetDefaultBackupDirectory();
        }

        public string TrashDirectory
        {
            set => _trashDirectory = value;
            get => _trashDirectory ?? GetDefaultTrashDirectory();
        }

        public static string ProgramDirectory => AppContext.BaseDirectory;

        private static string GetDirectory(params string[] directories)
        {
            foreach (var dir in directories.Select(FileUtilities.InjectEnvironmentVariables))
                if (Path.IsPathRooted(dir))
                    return dir;

            return null;
        }

        private static string GetDirectoryIfExists(params string[] directories)
        {
            foreach (var dir in directories.Select(FileUtilities.InjectEnvironmentVariables))
                if (Directory.Exists(dir))
                    return dir;

            return FileUtilities.InjectEnvironmentVariables(directories.Last());
        }

        private string GetDefaultConfigurationDirectory() => FileUtilities.GetExistingPathOrLast(
            Path.Join(AppDataDirectory, "Configurations"),
            Path.Join(ProgramDirectory, "Configurations"),
            Path.Join(System.Environment.CurrentDirectory, "Configurations")
        );

        private string GetDefaultSettingsFile() => Path.Join(AppDataDirectory, "settings.json");
        private string GetDefaultPluginDirectory() => Path.Join(AppDataDirectory, "Plugins");
        private string GetDefaultPresetDirectory() => Path.Join(AppDataDirectory, "Presets");
        private string GetDefaultTemporaryDirectory() => Path.Join(AppDataDirectory, "Temp");
        private string GetDefaultCacheDirectory() => Path.Join(AppDataDirectory, "Cache");
        private string GetDefaultBackupDirectory() => Path.Join(AppDataDirectory, "Backup");
        private string GetDefaultTrashDirectory() => Path.Join(AppDataDirectory, "Trash");
    }
}
