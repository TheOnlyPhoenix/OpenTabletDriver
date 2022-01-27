using System;
using System.Collections.Generic;
using OpenTabletDriver.Desktop.Interop;

namespace OpenTabletDriver.Desktop.Diagnostics
{
    public class EnvironmentDictionary : Dictionary<string, string>
    {
        public EnvironmentDictionary()
        {
            AddVariable("USER", "TEMP", "TMP", "TMPDIR");
            switch (DesktopInterop.CurrentPlatform)
            {
                case SystemPlatform.Linux:
                    AddVariable("DISPLAY", "WAYLAND_DISPLAY", "PWD", "PATH");
                    break;
                case SystemPlatform.Windows:
                    AddVariable("USERPROFILE");
                    break;
            }
        }

        private void AddVariable(params string[] variables)
        {
            foreach (var variable in variables)
            {
                var value = Environment.GetEnvironmentVariable(variable);
                base.Add(variable, value);
            }
        }
    }
}
