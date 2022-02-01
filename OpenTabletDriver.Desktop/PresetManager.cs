using System.Collections.Generic;
using System.IO;
using OpenTabletDriver.Desktop.Interop.AppInfo;

namespace OpenTabletDriver.Desktop
{
    public class PresetManager : IPresetManager
    {
        public PresetManager(IAppInfo appInfo)
        {
            _presetDirectory = new DirectoryInfo(appInfo.PresetDirectory);
        }

        private readonly DirectoryInfo _presetDirectory;

        private List<Preset> Presets { get; } = new List<Preset>();

        public IReadOnlyCollection<Preset> GetPresets() => Presets;

        public Preset FindPreset(string presetName)
        {
            return Presets.Find(preset => preset.Name == presetName);
        }

        private void Load()
        {
            foreach (var preset in _presetDirectory.EnumerateFiles("*.json"))
            {
                var settings = Settings.Deserialize(preset);
                if (settings != null)
                {
                    Presets.Add(new Preset(preset.Name.Replace(preset.Extension, string.Empty), settings));
                    Log.Write("Settings", $"Loaded preset '{preset.Name}'", LogLevel.Info);
                }
                else
                {
                    Log.Write("Settings", $"Invalid settings file '{preset.Name}' attempted to load into presets", LogLevel.Warning);
                }
            }
        }

        public void Refresh()
        {
            Presets.Clear();
            Load();
            Log.Write("Settings", $"Presets have been refreshed. Loaded {Presets.Count} presets.", LogLevel.Info);
        }
    }
}
