using System.Collections.Generic;

namespace OpenTabletDriver.Desktop;

public interface IPresetManager
{
    IReadOnlyCollection<Preset> GetPresets();
    Preset FindPreset(string presetName);
    void Refresh();
}
