using System;
using System.IO;
using System.Linq;

namespace OpenTabletDriver.Desktop.Interop.AppInfo
{
    public abstract class AppInfo : IAppInfo
    {
        private string configurationDirectory,
            settingsFile,
            pluginDirectory,
            presetDirectory,
            temporaryDirectory,
            cacheDirectory,
            backupDirectory,
            trashDirectory;

        public string AppDataDirectory { set; get; }

        public string ConfigurationDirectory
        {
            set => this.configurationDirectory = value;
            get => this.configurationDirectory ?? GetDefaultConfigurationDirectory();
        }

        public string SettingsFile
        {
            set => this.settingsFile = value;
            get => this.settingsFile ?? GetDefaultSettingsFile();
        }

        public string PluginDirectory
        {
            set => this.pluginDirectory = value;
            get => this.pluginDirectory ?? GetDefaultPluginDirectory();
        }

        public string PresetDirectory
        {
            set => this.presetDirectory = value;
            get => this.presetDirectory ?? GetDefaultPresetDirectory();
        }

        public string TemporaryDirectory
        {
            set => this.temporaryDirectory = value;
            get => this.temporaryDirectory ?? GetDefaultTemporaryDirectory();
        }

        public string CacheDirectory
        {
            set => this.cacheDirectory = value;
            get => this.cacheDirectory ?? GetDefaultCacheDirectory();
        }

        public string BackupDirectory
        {
            set => this.backupDirectory = value;
            get => this.backupDirectory ?? GetDefaultBackupDirectory();
        }

        public string TrashDirectory
        {
            set => this.trashDirectory = value;
            get => this.trashDirectory ?? GetDefaultTrashDirectory();
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
